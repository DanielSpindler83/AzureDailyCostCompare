using AzureDailyCostCompare.Domain;
using AzureDailyCostCompare.Infrastructure;

namespace AzureDailyCostCompare.Application;

/// <summary>
/// Main orchestrator that coordinates all cost comparison date calculations
/// APPLICATION: Facade service that orchestrates domain and application services
/// </summary>
public class CostComparisonDateService(
    DataAvailabilityService? dataAvailability = null,
    OverrideDateProcessor? overrideProcessor = null,
    MonthCalculationService? monthCalculation = null,
    ComparisonCalculationService? comparisonCalculation = null)
{
    private readonly DataAvailabilityService _dataAvailability = dataAvailability ?? new DataAvailabilityService();
    private readonly OverrideDateProcessor _overrideProcessor = overrideProcessor ?? new OverrideDateProcessor();
    private readonly MonthCalculationService _monthCalculation = monthCalculation ?? new MonthCalculationService();
    private readonly ComparisonCalculationService _comparisonCalculation = comparisonCalculation ?? new ComparisonCalculationService();

    /// <summary>
    /// Creates a comparison context using the current date and data availability cutoff
    /// </summary>
    /// <param name="cutoffHourUtc">UTC hour after which previous day's data is complete</param>
    /// <param name="dateProvider">Optional date provider for testing</param>
    public CostComparisonContext CreateContext(int cutoffHourUtc, IDateProvider? dateProvider = null)
    {
        var config = new DataAvailabilityConfig(cutoffHourUtc);
        var currentDate = (dateProvider ?? new UtcDateProvider()).GetCurrentDate();
        var referenceDate = _dataAvailability.GetLatestAvailableDataDate(currentDate, config);

        var (currentStart, previousStart, currentDays, previousDays) =
            _monthCalculation.CalculateMonthBoundaries(referenceDate);

        var comparisonDayCount = _comparisonCalculation.CalculateComparisonDayCount(
            null, currentDate, referenceDate);

        return new CostComparisonContext(
            referenceDate,
            ComparisonType.PartialMonth,
            currentStart,
            previousStart,
            currentDays,
            previousDays,
            comparisonDayCount,
            config);
    }

    /// <summary>
    /// Creates a comparison context for a specific override date
    /// </summary>
    /// <param name="cutoffHourUtc">UTC hour after which previous day's data is complete</param>
    /// <param name="overrideDate">Specific date to use for comparison calculations</param>
    /// <param name="dateProvider">Optional date provider for testing</param>
    public CostComparisonContext CreateContextWithOverride(
        int cutoffHourUtc,
        DateTime overrideDate,
        IDateProvider? dateProvider = null)
    {
        var config = new DataAvailabilityConfig(cutoffHourUtc);
        var currentDate = (dateProvider ?? new UtcDateProvider()).GetCurrentDate();

        var validatedDate = _dataAvailability.ValidateOverrideDate(overrideDate, currentDate, config);
        var processedDate = _overrideProcessor.ProcessOverrideDate(validatedDate, currentDate);

        var (currentStart, previousStart, currentDays, previousDays) =
            _monthCalculation.CalculateMonthBoundaries(processedDate);

        var comparisonType = _comparisonCalculation.DetermineComparisonType(overrideDate, currentDate);
        var comparisonDayCount = _comparisonCalculation.CalculateComparisonDayCount(
            overrideDate, currentDate, processedDate);

        return new CostComparisonContext(
            processedDate,
            comparisonType,
            currentStart,
            previousStart,
            currentDays,
            previousDays,
            comparisonDayCount,
            config);
    }

    /// <summary>
    /// Creates a service instance for testing with specific date parameters
    /// </summary>
    //public static CostComparisonContext CreateForTesting(int cutoffHourUtc, int year, int month, int day)
    //{
    //    var service = new CostComparisonDateService();
    //    var testDate = new DateTime(year, month, day);
    //    var mockDateProvider = new TestDateProvider(testDate);

    //    return service.CreateContextWithOverride(cutoffHourUtc, testDate, mockDateProvider);
    //}
}