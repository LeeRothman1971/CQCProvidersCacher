namespace Service.Contracts;

public interface IProviderService
{
    Task<MatchingProviders?> Get();
    Task<Provider?> Get(string id);
}