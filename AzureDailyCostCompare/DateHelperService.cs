﻿namespace AzureDailyCostCompare;

public class DateHelperService
{
    public DateTime Today { get; }
    public DateTime FirstDayOfPreviousMonth { get; }
    public DateTime FirstDayOfCurrentMonth { get; }
    public int CountOfDaysInPreviousMonth { get; }
    public int CountOfDaysInCurrentMonth { get; }

    public DateHelperService()
    {
        Today = DateTime.UtcNow;
        if (Today.Day == 1)
        {
            Console.WriteLine("Today is first of the month, no data yet, so using last day of previous month.");
            Today = Today.AddDays(-1);
        }

        FirstDayOfPreviousMonth = new DateTime(Today.Year, Today.Month, 1).AddMonths(-1);
        FirstDayOfCurrentMonth = new DateTime(Today.Year, Today.Month, 1);

        CountOfDaysInPreviousMonth = DateTime.DaysInMonth(FirstDayOfPreviousMonth.Year, FirstDayOfPreviousMonth.Month);
        CountOfDaysInCurrentMonth = DateTime.DaysInMonth(FirstDayOfCurrentMonth.Year, FirstDayOfCurrentMonth.Month);
    }
}
