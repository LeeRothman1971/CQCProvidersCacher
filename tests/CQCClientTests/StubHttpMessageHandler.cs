using System.Net;
using System.Text;

namespace CQCClientTests;

public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler;
    public List<HttpRequestMessage> Requests { get; } = new();

    public StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        this.handler = handler;
    }

    public StubHttpMessageHandler(HttpStatusCode status, string? body = null, string mediaType = "application/json")
        : this((_, _) => Task.FromResult(new HttpResponseMessage(status)
        {
            Content = body is null ? null : new StringContent(body, Encoding.UTF8, mediaType),
        }))
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return handler(request, cancellationToken);
    }
}