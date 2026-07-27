using System.Net;
using Aspire.Hosting.Testing;

namespace IntegrationTests;

public class AspireHostedApiTests
{
    [Fact]
    public async Task ApiGetById()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Aspire_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/providers/1-101630966");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiGet()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Aspire_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/providers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}