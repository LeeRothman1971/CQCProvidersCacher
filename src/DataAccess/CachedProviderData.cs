using DataAccess.Contracts;
using Microsoft.Azure.Cosmos;
using Service.Contracts;
using System.Net;
using System.Text.Json;

namespace DataAccess;

public class CachedProviderData : ICachedProviderData
{
    private readonly Container container;
    private readonly ICurrentDateTimeProvider currentDateTimeProvider;

    public CachedProviderData(CosmosClient cosmosClient, ICurrentDateTimeProvider currentDateTimeProvider)
    {
        var database = cosmosClient.GetDatabase("cqc");
        container = database.GetContainer("providers");
        this.currentDateTimeProvider = currentDateTimeProvider;
    }

    public async Task<Provider?> Get(string id)
    {
        try
        {
            var response = await container.ReadItemAsync<Provider>(id, new PartitionKey(id));

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task Save(Provider provider)
    {
        provider.Ttl = CalculateSecondsUntilAMonth();
        await container.UpsertItemAsync(provider, new PartitionKey(provider.ProviderId));
    }

    private int CalculateSecondsUntilAMonth()
    {
        return Convert.ToInt32(Math.Round((currentDateTimeProvider.GetCurrentUtc().AddMonths(1) - currentDateTimeProvider.GetCurrentUtc()).TotalSeconds));
    }
}