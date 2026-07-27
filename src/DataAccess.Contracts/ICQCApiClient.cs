using Service.Contracts;

namespace DataAccess.Contracts;

public interface ICQCApiClient
{
    Task<MatchingProviders?> GetMatchingProviders();
    Task<Provider?> GetProviderFromCQC(string id);
}