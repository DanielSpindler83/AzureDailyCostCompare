namespace AzureDailyCostCompare;

public interface IDateProvider
{
    DateTime GetCurrentDate();
}

public class UtcDateProvider : IDateProvider
{
    public DateTime GetCurrentDate() => DateTime.UtcNow;
}

public class FixedDateProvider(DateTime fixedDate) : IDateProvider
{
    private readonly DateTime _fixedDate = fixedDate;

    public DateTime GetCurrentDate() => _fixedDate;
}