using System.Collections.Immutable;

namespace Service.Contracts
{
    public record MatchingProviders
    {
        public required int Total { get; init; }
        public required int Page { get; init; }
        public int TotalPages { get; init; }
        public IImmutableList<ProviderSummary> Providers { get; init; }
    }

    public record ProviderSummary
    {
        public required string ProviderId { get; init; }
        public required string ProviderName { get; init; }
    }
}
