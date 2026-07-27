using System.Collections.Immutable;
using DataAccess.Contracts;
using NSubstitute;
using Service;
using Service.Contracts;

namespace ServiceUnitTests
{
    public class ProviderServiceTests
    {
        private const string id = "providerId";
        private readonly ProviderService sut;
        private readonly ICachedProviderData cachedData;
        private readonly ICQCApiClient cqcApiClient;

        public ProviderServiceTests()
        {
            cachedData = Substitute.For<ICachedProviderData>();
            cqcApiClient = Substitute.For<ICQCApiClient>();
            sut = new ProviderService(cachedData, cqcApiClient);
        }

        [Fact]
        public void ImplementsContract()
        {
            Assert.IsAssignableFrom<IProviderService>(sut);
        }

        [Fact]
        public async Task WhenGetById_ThenMustCallCachedDataStore()
        {
            await WhenForId();
            await cachedData.Received(1).Get(id);
        }

        [Fact]
        public async Task GivenProviderIsCached_WhenGet_ThenMustReturnProviderFromCachedDataStore()
        {
            var expectedResult = BuildProvider();
            cachedData.Get(id).Returns(expectedResult);

            var result = await WhenForId();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GivenProviderIsNotCached_WhenGet_ThenMustCallCQCApi()
        {
            SetUpForNotInCache();

            await WhenForId();
            await cqcApiClient.Received(1).GetProviderFromCQC(id);
        }

        [Fact]
        public async Task GivenProviderIsNotCached_WhenGet_ThenMustSaveProviderToCache()
        {
            SetUpForNotInCache();
            var cqcReturnedProvider = BuildProvider();
            cqcApiClient.GetProviderFromCQC(id).Returns(cqcReturnedProvider);
            
            await WhenForId();

            await cachedData.Received(1).Save(cqcReturnedProvider);
        }

        [Fact]
        public async Task GivenCqcReturnsNull_WhenGet_ThenMustNeverSaveProviderToCache()
        {
            SetUpForNotInCache();
            cqcApiClient.GetProviderFromCQC(id).Returns((Provider?)null);

            await WhenForId();

            await cachedData.DidNotReceive().Save(Arg.Any<Provider>());
        }

        [Fact]
        public async Task GivenProviderIsNotCached_WhenGet_ThenMustReturnProviderFromCqc()
        {
            SetUpForNotInCache();
            var cqcReturnedProvider = BuildProvider();
            cqcApiClient.GetProviderFromCQC(id).Returns(cqcReturnedProvider);

            var result = await WhenForId();

            Assert.Equal(cqcReturnedProvider, result);
        }

        [Fact]
        public async Task WhenGet_ThenMustReturnProviderListFromCqc()
        {
            var matchingProviders = new MatchingProviders
            {
                Total = 1,
                Page = 2,
                TotalPages = 3,
                Providers = ImmutableList.Create(new ProviderSummary
                {
                    ProviderId = "providerId",
                    ProviderName = "providerName"
                })
            };
            cqcApiClient.GetMatchingProviders().Returns(matchingProviders);
            var result = await sut.Get();

            Assert.Equal(matchingProviders, result);

        }

        private async Task<Provider?> WhenForId()
        {
            return await sut.Get(id);
        }

        private void SetUpForNotInCache()
        {
            cachedData.Get(id).Returns((Provider?)null);
        }

        private static Provider BuildProvider()
        {
            return new Provider
            {
                ProviderId = id,
                LocationIds = ImmutableList.Create<string>("location1", "location2"),
                OrganisationType = "OrgType"
            };
        }
    }
}
