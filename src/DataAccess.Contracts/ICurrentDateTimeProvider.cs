namespace DataAccess.Contracts;

public interface ICurrentDateTimeProvider
{
    DateTime GetCurrentUtc();
}