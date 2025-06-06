using AzureDailyCostCompare.Domain;


namespace AzureDailyCostCompare.Application;

/// <summary>
/// Main orchestrator that coordinates all cost comparison comparisonReferenceDate calculations
/// APPLICATION: Facade service that orchestrates domain and application services
/// </summary>
public class CostComparisonDateService(
    DataAvailabilityService dataAvailability, //rename this service to something more meaningful
    MonthCalculationService monthCalculation,
    ComparisonCalculationService comparisonCalculation)
{
    private readonly DataAvailabilityService _dataAvailability = dataAvailability;
    private readonly MonthCalculationService _monthCalculation = monthCalculation;
    private readonly ComparisonCalculationService _comparisonCalculation = comparisonCalculation;

    /// <summary>
    /// Creates a comparison context using the current comparisonReferenceDate and data availability cutoff
    /// </summary>
    /// <param name="previousDayUtcDataLoadDelayHours">UTC hour after which previous day's data is complete</param>
    /// <param name="dateProvider">Optional comparisonReferenceDate provider for testing</param>
    public CostComparisonContext CreateContext(DateTime comparisonReferenceDate, int previousDayUtcDataLoadDelayHours)
    {
        // we assume the comparisonReferenceDate is a valid comparisonReferenceDate once it gets here as its either passed (has a check) or we created the comparisonReferenceDate

        // work out the latest comparisonReferenceDate that has full data
        // check our comparisonReferenceDate is NOT later than latest full data comparisonReferenceDate - EXCEPTION if it is
        // is comparisonReferenceDate current month? true or false?
        // work out number of days to show in the comparison difference for partial vs full historical month
        // calculate the month boundaries and number of days
        // adjust comparisonReferenceDate if the comparisonReferenceDate is historical (so we compare full month) - changes comparisonReferenceDate to last day of the month for full months only

        var latestAvailableFullDaysDataDate = _dataAvailability.GetLatestAvailableFullDaysDataDate(previousDayUtcDataLoadDelayHours);
        var validatedDate = _dataAvailability.ValidateDate(comparisonReferenceDate, latestAvailableFullDaysDataDate, previousDayUtcDataLoadDelayHours);
        var comparisonType = _comparisonCalculation.DetermineComparisonType(comparisonReferenceDate);
        var comparisonDayCount = _comparisonCalculation.CalculateComparisonDayCount(comparisonReferenceDate, comparisonType);
        var (currentStart, previousStart, currentDays, previousDays) = _monthCalculation.CalculateMonthBoundaries(comparisonReferenceDate);
        var processedDate = _comparisonCalculation.AdjustForHistoricalDate(validatedDate); //sloppy as we may not need to do this check if the month is partial?

        // i feel this is all working now but it can be improved - capture state within the services? and only return what we need?

        return new CostComparisonContext(
            processedDate, // double check this is what we should pass in here
            comparisonType,
            currentStart,
            previousStart,
            currentDays,
            previousDays,
            comparisonDayCount,
            previousDayUtcDataLoadDelayHours); // i dont think this context needs previousDayUtcDataLoadDelayHours anymore - we handle it elsewhere....please check when you can...
    }


    /// <summary>
    /// Creates a comparison context using the current date and data availability cutoff
    /// </summary>
    /// <param name="previousDayUtcDataLoadDelayHours">UTC hour after which previous day's data is complete</param>
    /// <param name="dateProvider">Optional date provider for testing</param>
    //public CostComparisonContext CreateContextWithTodaysDate(int previousDayUtcDataLoadDelayHours)
    //{
    //    // we assume the comparisonReferenceDate is a valid comparisonReferenceDate once it gets here as its either passed (has a check) or we created the comparisonReferenceDate

    //    DateOnly latestAvailableDate = _dataAvailability.GetLatestAvailableFullDaysDataDate(CurrentDateTimeUtc, previousDayUtcDataLoadDelayHours);

    //    var (currentStart, previousStart, currentDays, previousDays) =
    //        _monthCalculation.CalculateMonthBoundaries(referenceDate);

    //    var comparisonDayCount = _comparisonCalculation.CalculateComparisonDayCount(
    //        null, CurrentDateTimeUtc, referenceDate);

    //    return new CostComparisonContext(
    //        referenceDate,
    //        ComparisonType.PartialMonth,
    //        currentStart,
    //        previousStart,
    //        currentDays,
    //        previousDays,
    //        comparisonDayCount,
    //        previousDayUtcDataLoadDelayHours); // i dont think this context needs previousDayUtcDataLoadDelayHours anymore - we handle it elsewhere....please check when you can...
    //}

    /// <summary>
    /// Creates a comparison context for a specific override date
    /// </summary>
    /// <param name="previousDayUtcDataLoadDelayHour">UTC hour after which previous day's data is complete</param>
    /// <param name="overrideDate">Specific date to use for comparison calculations</param>
    //public CostComparisonContext CreateContextWithOverride(
    //    int previousDayUtcDataLoadDelayHour,
    //    DateTime overrideDate)
    //{
    //    var validatedDate = _dataAvailability.ValidateOverrideDate(overrideDate, CurrentDateTimeUtc, previousDayUtcDataLoadDelayHour);
    //    var processedDate = _overrideProcessor.ProcessOverrideDate(validatedDate, CurrentDateTimeUtc);

    //    var (currentStart, previousStart, currentDays, previousDays) =
    //        _monthCalculation.CalculateMonthBoundaries(processedDate);

    //    var comparisonType = _comparisonCalculation.DetermineComparisonType(overrideDate, CurrentDateTimeUtc);
    //    var comparisonDayCount = _comparisonCalculation.CalculateComparisonDayCount(
    //        overrideDate, CurrentDateTimeUtc, processedDate);

    //    return new CostComparisonContext(
    //        processedDate,
    //        comparisonType,
    //        currentStart,
    //        previousStart,
    //        currentDays,
    //        previousDays,
    //        comparisonDayCount,
    //        previousDayUtcDataLoadDelayHour); // i dont think this context needs previousDayUtcDataLoadDelayHours anymore - we handle it elsewhere....please check when you can...
    //}
}