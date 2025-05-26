namespace AzureDailyCostCompare.Application.Interfaces
{
    public interface ICostComparisonDateService
    {
        int ComparisonTableDayCount { get; }
        DateTime CostDataReferenceDate { get; }
        int CurrentMonthDayCount { get; }
        DateTime CurrentMonthStartDate { get; }
        int DataAvailabilityCutoffHourUtc { get; }
        int PreviousMonthDayCount { get; }
        DateTime PreviousMonthStartDate { get; }

        (DateTime StartDate, DateTime EndDate) GetCurrentMonthDateRange();
        (DateTime StartDate, DateTime EndDate) GetPreviousMonthDateRange();
        bool IsCostDataAvailable(DateTime date);
    }
}