using System.Text.Json;
using DataAccess.Contracts;
using Microsoft.Azure.Cosmos;
using Service.Contracts;

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
        using var response = await container.ReadItemStreamAsync(id, new PartitionKey(id));
        return response.StatusCode == System.Net.HttpStatusCode.NotFound ?
            null
            : JsonSerializer.Deserialize<Provider>(response.Content);
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