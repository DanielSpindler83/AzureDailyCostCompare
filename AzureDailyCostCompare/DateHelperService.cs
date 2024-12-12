namespace AzureDailyCostCompare;

public interface IDateProvider
{
    DateTime GetCurrentDate();
}

public class UtcDateProvider : IDateProvider
{
    public DateTime GetCurrentDate() => DateTime.UtcNow;
}

public class FixedDateProvider : IDateProvider
{
    private readonly DateTime _fixedDate;

    public FixedDateProvider(DateTime fixedDate)
    {
        _fixedDate = fixedDate;
    }

    public DateTime GetCurrentDate() => _fixedDate;
}

public class DateHelperService
{
    public DateTime Today { get; }
    public DateTime FirstDayOfPreviousMonth { get; }
    public DateTime FirstDayOfCurrentMonth { get; }
    public int CountOfDaysInPreviousMonth { get; }
    public int CountOfDaysInCurrentMonth { get; }

    public DateHelperService(IDateProvider? dateProvider = null)
    {
        // Use provided date provider or default to UTC now
        var currentDate = (dateProvider ?? new UtcDateProvider()).GetCurrentDate();

        // If the provided/current date is the first of the month, 
        // adjust to the last day of the previous month
        if (currentDate.Day == 1)
        {
            Console.WriteLine("Today is first of the month, no data yet, so using last day of previous month.");
            currentDate = currentDate.AddDays(-1);
        }

        Today = currentDate;
        FirstDayOfPreviousMonth = new DateTime(Today.Year, Today.Month, 1).AddMonths(-1);
        FirstDayOfCurrentMonth = new DateTime(Today.Year, Today.Month, 1);
        CountOfDaysInPreviousMonth = DateTime.DaysInMonth(FirstDayOfPreviousMonth.Year, FirstDayOfPreviousMonth.Month);
        CountOfDaysInCurrentMonth = DateTime.DaysInMonth(FirstDayOfCurrentMonth.Year, FirstDayOfCurrentMonth.Month);
    }

    // Static method for easy testing creation
    public static DateHelperService CreateForTesting(int year, int month, int day)
    {
        return new DateHelperService(new FixedDateProvider(new DateTime(year, month, day)));
    }
}