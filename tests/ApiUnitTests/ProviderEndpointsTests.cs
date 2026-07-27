using System.Collections.Immutable;
using Api;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Service.Contracts;

namespace ApiUnitTests
{
    public class ProviderEndpointsTests
    {
        private const string providerId = "providerId";
        private readonly IProviderService service;

        public ProviderEndpointsTests()
        {
            service = Substitute.For<IProviderService>();
        }

        [Fact]
        public async Task WhenGet_ThenMustCallService()
        {
            
            await ProviderEndpoints.Get(service);
            await service.Received(1).Get();
        }

        [Fact]
        public async Task WhenGet_ThenMustReturnOkResultWithProviderList()
        {
            var expectedResult = new MatchingProviders
            {
                Total = 10,
                Page = 1
            };
            service.Get()!.Returns(Task.FromResult(expectedResult));
            var result = await ProviderEndpoints.Get(service);

            Assert.NotNull(result);
            Assert.IsType<Ok<MatchingProviders>>(result.Result);
            
            var model = result.Result as Ok<MatchingProviders>;
            Assert.Equal(expectedResult.Total, model!.Value!.Total);
            Assert.Equal(expectedResult.Page, model.Value!.Page);
        }

        [Fact]
        public async Task WhenGet_AndServiceReturnsNull_ThenMustReturnNotFound()
        {
            service.Get().Returns(Task.FromResult<MatchingProviders?>(null));
            var result = await ProviderEndpoints.Get(service);

            Assert.NotNull(result);
            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task WhenGetById_ThenMustCallService()
        {
            await ProviderEndpoints.GetById(providerId, service);
            await service.Received(1).Get(providerId);
        }

        [Fact]
        public async Task WhenGetById_ThenMustReturnOkResultWithProvider()
        {
            var expectedResult = new Provider
            {
                ProviderId = providerId,
                LocationIds = ImmutableList.Create<string>("location1", "location2"),
                OrganisationType = "OrganisationType"
            };
            service.Get(providerId)!.Returns(Task.FromResult(expectedResult));
            var result = await ProviderEndpoints.GetById(providerId, service);

            Assert.NotNull(result);
            Assert.IsType<Ok<Provider>>(result.Result);

            var model = result.Result as Ok<Provider>;
            Assert.Equal(expectedResult.ProviderId, model!.Value!.ProviderId);
            Assert.Equal(expectedResult.LocationIds, model.Value!.LocationIds);
            Assert.Equal(expectedResult.OrganisationType, model.Value!.OrganisationType);
        }

        [Fact]
        public async Task WhenGetById_AndServiceReturnsNull_ThenMustReturnNotFound()
        {
            service.Get(providerId).Returns(Task.FromResult<Provider?>(null));
            var result = await ProviderEndpoints.GetById(providerId, service);

            Assert.NotNull(result);
            Assert.IsType<NotFound>(result.Result);
        }
    }

}
