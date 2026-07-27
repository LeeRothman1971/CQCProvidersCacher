using System.Diagnostics.CodeAnalysis;
using DataAccess;
using DataAccess.Contracts;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<CosmosClient>(_ =>
        {
            var cosmosClientOptions = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway,
                LimitToEndpoint = true,
                UseSystemTextJsonSerializerWithOptions = CreateJsonSerializerOptions(),
                HttpClientFactory = () => new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                })  
            };

            var cosmosClient = new CosmosClient(connectionString, cosmosClientOptions);
            return cosmosClient;
        });
        services.AddHostedService<CosmosDbInitializationService>();
        services.AddSingleton<ICachedProviderData, CachedProviderData>();
        return services;
    }

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        return serializerOptions;
    }
}