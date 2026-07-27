#pragma warning disable ASPIRECOSMOSDB001
#pragma warning disable ASPIRECERTIFICATES001

var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(emulator =>
    {
        emulator.WithDataExplorer();
    });

builder.AddProject<Projects.Api>("api").WithExternalHttpEndpoints()
    .WithReference(cosmos)
    .WaitFor(cosmos);

builder.Build().Run();

#pragma warning restore ASPIRECOSMOSDB001
#pragma warning restore ASPIRECERTIFICATES001
