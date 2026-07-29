using System.Collections.Immutable;
using System.Net;
using System.Text.Json;
using DataAccess;
using DataAccess.Contracts;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using Service.Contracts;

namespace DataAccessTests
{
    public class CachedProviderDataTests
    {
        private const string providerId = "provider-123";
        private const string databaseName = "cqc";
        private const string containerName = "providers";
        private readonly PartitionKey partitionKey;
        private readonly ICurrentDateTimeProvider currentDateTimeProvider;

        private readonly CachedProviderData sut;
        private readonly CosmosClient cosmosClient;
        private readonly Database database;
        private readonly Container container;

        public CachedProviderDataTests()
        {
            cosmosClient = Substitute.For<CosmosClient>();
            database = Substitute.For<Database>();
            container = Substitute.For<Container>();
            partitionKey = new PartitionKey(providerId);
            currentDateTimeProvider = Substitute.For<ICurrentDateTimeProvider>();

            cosmosClient.GetDatabase(databaseName).Returns(database);
            database.GetContainer(containerName).Returns(container);
            currentDateTimeProvider.GetCurrentUtc().Returns(new DateTime(2023, 1, 1, 8, 23, 44, DateTimeKind.Utc));

            sut = new CachedProviderData(cosmosClient, currentDateTimeProvider);
        }

        [Fact]
        public void ImplementsContract()
        {
            Assert.IsAssignableFrom<ICachedProviderData>(sut);
        }

        [Fact]
        public async Task WhenGet_ThenMustCallReadItemStreamAsync()
        {
            var itemResponse = CreateOkResponse(BuildProvider());
            container.ReadItemAsync<Provider>(providerId, partitionKey).Returns(itemResponse);

            await sut.Get(providerId);

            await container.Received(1).ReadItemAsync<Provider>(providerId, partitionKey);
        }

        [Fact]
        public async Task GivenProviderExists_WhenGet_ThenMustReturnProvider()
        {
            var expectedProvider = BuildProvider();
            var itemResponse = CreateOkResponse(expectedProvider);
            container.ReadItemAsync<Provider>(providerId, partitionKey).Returns(itemResponse);

            var result = await sut.Get(providerId);

            Assert.NotNull(result);
            Assert.Equal(expectedProvider.ProviderId, result?.ProviderId);
        }

        [Fact]
        public async Task GivenProviderDoesNotExist_WhenGet_ThenMustReturnNull()
        {
            var itemResponse = Substitute.For<ItemResponse<Provider>>();
            itemResponse.StatusCode.Returns(HttpStatusCode.NotFound);
            container.ReadItemAsync<Provider>(providerId, partitionKey).Returns(itemResponse);

            var result = await sut.Get(providerId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GivenProvider_WhenSave_ThenMustCallUpsertItemAsync()
        {
            var provider = BuildProvider();
            var itemResponse = Substitute.For<ItemResponse<Provider>>();

            container.UpsertItemAsync(provider, partitionKey)
                .Returns(itemResponse);

            await sut.Save(provider);

            await container.Received(1).UpsertItemAsync(provider, partitionKey);
        }

        [Fact]
        public async Task WhenSave_ThenMustUpsertWithProviderIdAsPartitionKey()
        {
            var provider = BuildProvider();
            var itemResponse = Substitute.For<ItemResponse<Provider>>();

            container.UpsertItemAsync(provider, partitionKey)
                .Returns(itemResponse);

            await sut.Save(provider);

            await container.Received(1).UpsertItemAsync(provider, partitionKey);
        }

        [Fact]
        public async Task WhenSave_ThenTtlMustBeOneMonthDurationInSeconds()
        {
            var provider = BuildProvider();
            Provider? savedProvider = null;
            var response = Substitute.For<ItemResponse<Provider>>();

            container.UpsertItemAsync(Arg.Do<Provider>(p => savedProvider = p), partitionKey)
                .Returns(response);

            await sut.Save(provider);

            Assert.Equal(2678400, savedProvider!.Ttl);
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

        private static ItemResponse<Provider> CreateOkResponse(Provider provider)
        {
            var itemResponse = Substitute.For<ItemResponse<Provider>>();
            itemResponse.StatusCode.Returns(HttpStatusCode.OK);
            itemResponse.Resource.Returns(provider);
            return itemResponse;
        }
    }
}