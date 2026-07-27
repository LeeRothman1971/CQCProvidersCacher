using Aspire.Hosting;
using Aspire.Hosting.Testing;
using DataAccess;
using Microsoft.Azure.Cosmos;
using Service.Contracts;
using System.Collections.Immutable;
using Aspire.Hosting.ApplicationModel;
using Xunit.Sdk;

namespace DataAccessTests;

public class AppHostFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;
    public async Task InitializeAsync()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<AppHost>(
            ["SystemUnderTest=true"], cts.Token);
        App = await appHost.BuildAsync(cts.Token);
        await App.ResourceNotifications.WaitForResourceAsync("cosmos", KnownResourceStates.Running, cts.Token);
        await App.StartAsync(cts.Token);
    }

    public async Task DisposeAsync() => await App.DisposeAsync();
}

public class ProvidersTests : IClassFixture<AppHostFixture>, IDisposable
{
    private const string providerId = "provider-123";
    private readonly CachedProviderData sut;
    private CosmosClient? cosmosClient;
    private ContainerResponse? containerResponse;

    public ProvidersTests(AppHostFixture fixture)
    {
        Task.Run(() => InitializeDataStore(fixture)).Wait();
        sut = new CachedProviderData(cosmosClient, new CurrentDateTimeProvider());
    }

    private async Task InitializeDataStore(AppHostFixture fixture)
    {
        string? cosmosEndpointStr = null;
        try
        {
            cosmosEndpointStr = fixture.App.GetEndpoint("cosmos", "https").ToString();
        }
        catch (ArgumentException)
        {
            try
            {
                cosmosEndpointStr = fixture.App.GetEndpoint("cosmos", "http").ToString();
            }
            catch (ArgumentException)
            {
                // fallback to connection string if no explicit endpoint name is available
                var conn = await fixture.App.GetConnectionStringAsync("cosmos",
                    System.Threading.CancellationToken.None);
                // parse AccountEndpoint=...; from connection string
                var parts = conn.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    if (p.StartsWith("AccountEndpoint=", StringComparison.OrdinalIgnoreCase))
                    {
                        cosmosEndpointStr = p.Substring("AccountEndpoint=".Length);
                        break;
                    }
                }
            }
        }

        if (string.IsNullOrWhiteSpace(cosmosEndpointStr))
        {
            throw new InvalidOperationException("Could not determine cosmos endpoint from Aspire test host.");
        }

        cosmosClient = new CosmosClient(accountEndpoint: cosmosEndpointStr,
            authKeyOrResourceToken:
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
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
                ConnectionMode = ConnectionMode.Gateway
            });
    }

    [Fact]
    public async Task WhenSave_ThenMustPersist()
    {
        var provider = BuildProvider();
        await sut.Save(provider);
    }

    private static Provider BuildProvider()
    {
        return new Provider
        {
            ProviderId = providerId,
            LocationIds = ImmutableList.Create("loc-1", "loc-2"),
            OrganisationType = "Charity",
            Name = "Test Provider",
            RegistrationStatus = "Registered"
        };
    }

    public void Dispose()
    {
        cosmosClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}