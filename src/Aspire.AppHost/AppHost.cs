#pragma warning disable ASPIRECOSMOSDB001
#pragma warning disable ASPIRECERTIFICATES001

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(emulator =>
    {
        emulator.WithDataExplorer(1234).WithGatewayPort(8081);
    }).WithAccessKeyAuthentication();

var systemUnderTest = builder.Configuration.GetValue("SystemUnderTest", false);
if (!systemUnderTest)
{
    builder.AddProject<Projects.Api>("api").WithExternalHttpEndpoints()
        .WithReference(cosmos)
        .WaitFor(cosmos);
}

builder.Build().Run();

[ExcludeFromCodeCoverage]
public class AppHost { }

#pragma warning restore ASPIRECOSMOSDB001
#pragma warning restore ASPIRECERTIFICATES001
