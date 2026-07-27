using System.Collections.Immutable;
using System.Net;
using System.Text.Json;
using Service.Contracts;

namespace CQCClientTests
{
    public class CQCClientTests
    {
        private readonly MatchingProviders matchingProviders;
        private protected StubHttpMessageHandler Handler;
        private readonly CQCApiClient.CQCApiClient sut;

        public CQCClientTests()
        {
            matchingProviders = new MatchingProviders
            {
                Total = 1,
                Page = 1,
                TotalPages = 1,
                Providers = ImmutableList.Create(new ProviderSummary
                {
                    ProviderId = "123",
                    ProviderName = "Test Provider"
                })
            };

            Handler = new StubHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(matchingProviders));
            var httpClient1 = new HttpClient(Handler)
            {
                BaseAddress = new Uri("https://api.test.com/")
            };
            sut = new CQCApiClient.CQCApiClient(httpClient1);
        }

        [Fact]
        public async Task GivenClientReturnsMatchingProviders_WhenGetThenMustReturnResult()
        {
            var result = await sut.GetMatchingProviders();

            Assert.Equal(matchingProviders.TotalPages, result!.TotalPages);
            Assert.Equal(matchingProviders.Total, result.Total);

            var sent = Assert.Single(Handler.Requests);
            Assert.Equal(HttpMethod.Get, sent.Method);
            Assert.Equal("/public/v1/providers", sent.RequestUri!.AbsolutePath);
        }

        [Fact]
        public async Task GivenClientReturnsNotFound_WhenGetMatchingProviders_ThenMustReturnNull()
        {
            Handler = new StubHttpMessageHandler(HttpStatusCode.NotFound, string.Empty);
            var httpClient = new HttpClient(Handler)
            {
                BaseAddress = new Uri("https://api.test.com/")
            };
            var client = new CQCApiClient.CQCApiClient(httpClient);

            var result = await client.GetMatchingProviders();

            Assert.Null(result);
        }

        [Fact]
        public async Task GivenClientReturnsProvider_WhenGetProviderFromCQC_ThenMustReturnResult()
        {
            var providerId = "123";
            var provider = new Provider
            {
                ProviderId = providerId,
                LocationIds = ImmutableList.Create("location1"),
                OrganisationType = "Type1"
            };

            Handler = new StubHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(provider));
            var httpClient = new HttpClient(Handler)
            {
                BaseAddress = new Uri("https://api.test.com/")
            };
            var client = new CQCApiClient.CQCApiClient(httpClient);

            var result = await client.GetProviderFromCQC(providerId);

            Assert.NotNull(result);
            Assert.Equal(provider.ProviderId, result.ProviderId);
            Assert.Equal(provider.OrganisationType, result.OrganisationType);

            var sent = Assert.Single(Handler.Requests);
            Assert.Equal(HttpMethod.Get, sent.Method);
            Assert.Equal($"/public/v1/providers/{providerId}", sent.RequestUri!.AbsolutePath);
        }

        [Fact]
        public async Task GivenClientReturnsNotFound_WhenGetProviderFromCQC_ThenMustReturnNull()
        {
            var providerId = "123";
            Handler = new StubHttpMessageHandler(HttpStatusCode.NotFound, string.Empty);
            var httpClient = new HttpClient(Handler)
            {
                BaseAddress = new Uri("https://api.test.com/")
            };
            var client = new CQCApiClient.CQCApiClient(httpClient);

            var result = await client.GetProviderFromCQC(providerId);

            Assert.Null(result);
        }
    }
}
