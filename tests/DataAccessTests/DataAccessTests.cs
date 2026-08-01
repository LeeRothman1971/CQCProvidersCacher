using Aspire.Hosting;
using Aspire.Hosting.Testing;
using DataAccess;
using Microsoft.Azure.Cosmos;
using Service.Contracts;
using System.Collections.Immutable;

namespace DataAccessTests;

public class AppHostFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;
    public async Task InitializeAsync()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<AppHost>(
            ["SystemUnderTest=true"], cts.Token);
        App = await appHost.BuildAsync(cts.Token);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("cosmos", cts.Token);
        await App.StartAsync(cts.Token);
    }

    public async Task DisposeAsync() => await App.DisposeAsync();
}

public class ProvidersTests : IClassFixture<AppHostFixture>, IDisposable
{
    private string providerId = "provider-123";
    private readonly CachedProviderData sut;
    private CosmosClient? cosmosClient;
    private DatabaseResponse? databaseResponse;

    public ProvidersTests(AppHostFixture fixture)
    {
        Task.Run(() => InitializeDataStore(fixture)).Wait();
        sut = new CachedProviderData(cosmosClient, new CurrentDateTimeProvider());
    }

    private async Task InitializeDataStore(AppHostFixture fixture)
    {
        var connectionString = await fixture.App.GetConnectionStringAsync("cosmos");
        cosmosClient =
            new CosmosClient(
                connectionString: connectionString,
                new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        IgnoreNullValues = true,
                        Indented = true,
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    },
                    HttpClientFactory = () => new HttpClient(new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    }),
                    ConnectionMode = ConnectionMode.Gateway,
                    LimitToEndpoint = true
                });

        databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync("cqc");
        var containerProperties = new ContainerProperties("providers", "/providerId")
        {
            DefaultTimeToLive = -1
        };
        var throughPut = ThroughputProperties.CreateAutoscaleThroughput(1000);
        await databaseResponse.Database.CreateContainerIfNotExistsAsync(containerProperties, throughPut);
    }

    [Fact]
    public async Task WhenSave_ThenMustPersist()
    {
        var provider = BuildProvider();
        await sut.Save(provider);

        var result = await databaseResponse.Database.GetContainer("providers").ReadItemAsync<Provider>(provider.ProviderId, new PartitionKey(provider.ProviderId));
        Assert.Equal(provider.ProviderId, result.Resource.ProviderId);
    }

    [Fact]
    public async Task GivenProviderDoesNotExist_WhenGetThenMustReturnNull()
    {
        var result = await sut.Get("pid");
        Assert.Null(result);
    }

    [Fact]
    public async Task GivenProviderDoesExist_WhenGetThenMustReturnProvider()
    {
        providerId = "new provider id";
        var provider = BuildProvider();
        var container = databaseResponse.Database.GetContainer("providers");
        await container.CreateItemAsync(provider, new PartitionKey(provider.ProviderId));

        var result = await sut.Get(provider.ProviderId);
        Assert.Equal(provider.ProviderId, result.ProviderId);
    }

    private Provider BuildProvider()
    {
        return new Provider
        {
            ProviderId = providerId,
            LocationIds = ImmutableList.Create("loc-1", "loc-2"),
            OrganisationType = "Charity",
            Name = "Test Provider",
            RegistrationStatus = "Registered",
            Ttl = 123456
        };
    }

    public void Dispose()
    {
        cosmosClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}