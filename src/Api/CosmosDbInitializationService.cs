using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Cosmos;

namespace Api;

[ExcludeFromCodeCoverage]
public class CosmosDbInitializationService : IHostedService
{
    private readonly CosmosClient cosmosClient;

    public CosmosDbInitializationService(CosmosClient cosmosClient)
    {
        this.cosmosClient = cosmosClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync("cqc", cancellationToken: cancellationToken);
        var containerProperties = new ContainerProperties("providers", "/providerId")
        {
            DefaultTimeToLive = -1
        };
        var throughPut = ThroughputProperties.CreateAutoscaleThroughput(1000);
        await database.Database.CreateContainerIfNotExistsAsync(containerProperties, throughPut, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}