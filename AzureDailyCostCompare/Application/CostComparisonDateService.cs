using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

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

    public CostComparisonContext CreateContext(DateTime inputComparisonDate, int previousDayUtcDataLoadDelayHours)
    {
        var latestAvailableFullDaysDataDate = _dataAvailability.GetLatestAvailableFullDaysDataDate(previousDayUtcDataLoadDelayHours);
        var validatedComparisonDate = _dataAvailability.ValidateDate(inputComparisonDate, latestAvailableFullDaysDataDate);
        var comparisonDayCount = _comparisonCalculation.CalculateComparisonDayCount(validatedComparisonDate);
        var monthComparisonPeriod = _monthCalculation.CalculateMonthComparisonPeriod(validatedComparisonDate);
        var processedComparisonDate = _comparisonCalculation.AdjustForHistoricalDate(validatedComparisonDate);

        return new CostComparisonContext(
            comparisonReferenceDate: processedComparisonDate,
            monthComparisonPeriod: monthComparisonPeriod,
            comparisonTableDayCount: comparisonDayCount,
            dataLoadDelayHours: previousDayUtcDataLoadDelayHours);
    }
}