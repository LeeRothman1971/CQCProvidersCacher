using DataAccess.Contracts;
using Service.Contracts;

namespace Service
{
    public class ProviderService : IProviderService
    {
        private readonly ICachedProviderData cachedData;
        private readonly ICQCApiClient cqcApiClient;

        public ProviderService(ICachedProviderData cachedData, ICQCApiClient cqcApiClient)
        {
            this.cachedData = cachedData;
            this.cqcApiClient = cqcApiClient;
        }

        public async Task<MatchingProviders?> Get()
        {
            return await cqcApiClient.GetMatchingProviders();
        }

        public async Task<Provider?> Get(string id)
        {
            var provider = await cachedData.Get(id);
            if (provider is not null)
                return provider;

            var cqcReturnedProvider = await cqcApiClient.GetProviderFromCQC(id);
            if (cqcReturnedProvider != null)
            {
                await cachedData.Save(cqcReturnedProvider);
            }
            return cqcReturnedProvider;
        }
    }
}
