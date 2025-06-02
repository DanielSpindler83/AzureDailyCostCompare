using AzureDailyCostCompare.Domain;


namespace AzureDailyCostCompare.Application;

/// <summary>
/// Main orchestrator that coordinates all cost comparison date calculations
/// APPLICATION: Facade service that orchestrates domain and application services
/// </summary>
public class CostComparisonDateService(
    DataAvailabilityService dataAvailability, //rename this service to something more meaningful
    OverrideDateProcessor overrideProcessor,
    MonthCalculationService monthCalculation,
    ComparisonCalculationService comparisonCalculation)
{
    private readonly DataAvailabilityService _dataAvailability = dataAvailability;
    private readonly OverrideDateProcessor _overrideProcessor = overrideProcessor;
    private readonly MonthCalculationService _monthCalculation = monthCalculation;
    private readonly ComparisonCalculationService _comparisonCalculation = comparisonCalculation;

    public DateTime CurrentDateTimeUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a comparison context using the current date and data availability cutoff
    /// </summary>
    /// <param name="previousDayUtcDataLoadDelayHours">UTC hour after which previous day's data is complete</param>
    /// <param name="dateProvider">Optional date provider for testing</param>
    public CostComparisonContext CreateContext(int previousDayUtcDataLoadDelayHours)
    {
        var referenceDate = _dataAvailability.GetLatestAvailableDataDate(CurrentDateTimeUtc, previousDayUtcDataLoadDelayHours);

        var (currentStart, previousStart, currentDays, previousDays) =
            _monthCalculation.CalculateMonthBoundaries(referenceDate);

        var comparisonDayCount = _comparisonCalculation.CalculateComparisonDayCount(
            null, CurrentDateTimeUtc, referenceDate);

        return new CostComparisonContext(
            referenceDate,
            ComparisonType.PartialMonth,
            currentStart,
            previousStart,
            currentDays,
            previousDays,
            comparisonDayCount,
            previousDayUtcDataLoadDelayHours); // i dont think this context needs previousDayUtcDataLoadDelayHours anymore - we handle it elsewhere....please check when you can...
    }

    /// <summary>
    /// Creates a comparison context for a specific override date
    /// </summary>
    /// <param name="previousDayUtcDataLoadDelayHour">UTC hour after which previous day's data is complete</param>
    /// <param name="overrideDate">Specific date to use for comparison calculations</param>
    public CostComparisonContext CreateContextWithOverride(
        int previousDayUtcDataLoadDelayHour,
        DateTime overrideDate)
    {
        var validatedDate = _dataAvailability.ValidateOverrideDate(overrideDate, CurrentDateTimeUtc, previousDayUtcDataLoadDelayHour);
        var processedDate = _overrideProcessor.ProcessOverrideDate(validatedDate, CurrentDateTimeUtc);

        var (currentStart, previousStart, currentDays, previousDays) =
            _monthCalculation.CalculateMonthBoundaries(processedDate);

        var comparisonType = _comparisonCalculation.DetermineComparisonType(overrideDate, CurrentDateTimeUtc);
        var comparisonDayCount = _comparisonCalculation.CalculateComparisonDayCount(
            overrideDate, CurrentDateTimeUtc, processedDate);

        return new CostComparisonContext(
            processedDate,
            comparisonType,
            currentStart,
            previousStart,
            currentDays,
            previousDays,
            comparisonDayCount,
            previousDayUtcDataLoadDelayHour); // i dont think this context needs previousDayUtcDataLoadDelayHours anymore - we handle it elsewhere....please check when you can...
    }
}