using System.Diagnostics.CodeAnalysis;
using Api;
using DataAccess;
using DataAccess.Contracts;
using Scalar.AspNetCore;
using Service;
using Service.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddTransient<ICQCApiClient, CQCApiClient.CQCApiClient>();
builder.Services.AddTransient<ICurrentDateTimeProvider, CurrentDateTimeProvider>();
var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTIONSTRING");
builder.Services.AddCosmosDb(connectionString!);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/api-docs");
}

app.MapGet("/", () => Results.Redirect("/api-docs"));
ProviderEndpoints.Map(app);
app.MapDefaultEndpoints();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }