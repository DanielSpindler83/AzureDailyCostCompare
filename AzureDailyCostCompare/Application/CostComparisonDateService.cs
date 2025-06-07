using AzureDailyCostCompare.Domain;


namespace AzureDailyCostCompare.Application;

/// <summary>
/// Main orchestrator that coordinates all cost comparison comparisonReferenceDate calculations
/// APPLICATION: Facade service that orchestrates domain and application services
/// </summary>
public class CostComparisonDateService
{
    private readonly DataAvailabilityService _dataAvailability;
    private readonly MonthCalculationService _monthCalculation;
    private readonly ComparisonCalculationService _comparisonCalculation;

    public CostComparisonDateService(
        DataAvailabilityService dataAvailability,
        MonthCalculationService monthCalculation,
        ComparisonCalculationService comparisonCalculation)
    {
        _dataAvailability = dataAvailability;
        _monthCalculation = monthCalculation;
        _comparisonCalculation = comparisonCalculation;
    }

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
        var monthComparisonPeriod = _monthCalculation.CalculateMonthComparisonPeriod(comparisonReferenceDate);
        var processedDate = _comparisonCalculation.AdjustForHistoricalDate(validatedDate); //sloppy as we may not need to do this check if the month is partial?

        // i feel this is all working now but it can be improved - capture state within the services? and only return what we need?

        return new CostComparisonContext(
            processedDate, 
            comparisonType,
            monthComparisonPeriod,
            comparisonDayCount,
            previousDayUtcDataLoadDelayHours); // i dont think this context needs previousDayUtcDataLoadDelayHours anymore - we handle it elsewhere....please check when you can...
    }
}