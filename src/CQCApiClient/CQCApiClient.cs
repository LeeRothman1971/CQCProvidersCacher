using System.Net;
using System.Net.Http.Json;
using DataAccess.Contracts;
using Service.Contracts;

namespace CQCApiClient;

public class CQCApiClient : ICQCApiClient
{
    private const string baseUrl = "https://api.service.cqc.org.uk/public/v1/";
    private const string ocpApimSubscriptionKey = "Ocp-Apim-Subscription-Key";
    private const string subscriptionKey = "65907e17e06440f6b212ded670f54cbb";
    private readonly HttpClient httpClient;

    public CQCApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        this.httpClient.DefaultRequestHeaders.Add(ocpApimSubscriptionKey, subscriptionKey);
        this.httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<MatchingProviders?> GetMatchingProviders()
    {
        httpClient.DefaultRequestHeaders.Add(ocpApimSubscriptionKey, subscriptionKey);
        httpClient.BaseAddress = new Uri(baseUrl);

        using var response = await httpClient.GetAsync("providers").ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        var matchingProviders = await response.Content.ReadFromJsonAsync<MatchingProviders?>().ConfigureAwait(false);

        return matchingProviders;
    }

    public async Task<Provider?> GetProviderFromCQC(string id)
    {
        using var response = await httpClient.GetAsync($"providers/{id}").ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        var provider = await response.Content.ReadFromJsonAsync<Provider?>().ConfigureAwait(false);

        return provider;
    }
}