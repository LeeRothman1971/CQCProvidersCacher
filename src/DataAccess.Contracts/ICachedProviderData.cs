using Service.Contracts;

namespace DataAccess.Contracts;

public interface ICachedProviderData
{
    Task<Provider?> Get(string id);
    Task Save(Provider provider);
}