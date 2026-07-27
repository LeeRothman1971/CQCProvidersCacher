using System.Diagnostics.CodeAnalysis;
using DataAccess.Contracts;

namespace DataAccess;

[ExcludeFromCodeCoverage]
public class CurrentDateTimeProvider : ICurrentDateTimeProvider
{
    public DateTime GetCurrentUtc()
    {
        return DateTime.UtcNow;
    }
}