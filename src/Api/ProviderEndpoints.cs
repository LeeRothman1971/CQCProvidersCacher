using Microsoft.AspNetCore.Http.HttpResults;
using Service.Contracts;

namespace Api
{
    public static class ProviderEndpoints
    {
        public static void Map(WebApplication app)
        {
            app.MapGet("/providers", Get);
            app.MapGet("/providers/{id:}", GetById);
        }

        public static async Task<Results<Ok<MatchingProviders>, NotFound>> Get(IProviderService service)
        {
            var response = await service.Get();
            return response == null ? TypedResults.NotFound() : TypedResults.Ok(response);
        }

        public static async Task<Results<Ok<Provider>, NotFound>> GetById(string id, IProviderService service)
        {
            var response = await service.Get(id);
            return response == null ? TypedResults.NotFound() : TypedResults.Ok(response);
        }

    }
}
