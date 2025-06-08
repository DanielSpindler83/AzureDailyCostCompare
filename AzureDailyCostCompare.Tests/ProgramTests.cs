using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Tests;

public class ProgramTests : ReportGeneratorTestBase
{
    [Fact]
    public void ReportGenerator_Should_Generate_Expected_Output_Dec2023_Jan2024()
    {
        // Arrange: Load mock cost data from file
        var mockCostData = TestHelper.LoadMockCostData("mockCostData_dec2023-jan2024.json");

        // Arrange: Setup cost comparision context
        var comparisonReferenceDate = new DateTime(2024, 01, 01);
        var monthCalculationService = new MonthCalculationService();
        var monthComparisonPeriod = monthCalculationService.CalculateMonthComparisonPeriod(comparisonReferenceDate);
        var comparisonCalculation = new ComparisonCalculationService();

        var costComparisonContext = new CostComparisonContext(
            comparisonReferenceDate: comparisonReferenceDate,
            monthComparisonPeriod: monthComparisonPeriod,
            comparisonTableDayCount: comparisonCalculation.CalculateComparisonDayCount(comparisonReferenceDate),
            dataLoadDelayHours: 4
        );

        // Arrange: Initialize the report generator
        // we get this from base class - the service collection and report generator

        // Capture console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Act: Generate report
        base.ReportGenerator.GenerateDailyCostReport(mockCostData, costComparisonContext, showWeeklyPatterns: false, showDayOfWeekAverages: false);

        // Assert: Validate expected report output
        var output = consoleOutput.ToString();
        Assert.Contains("December", output);
        Assert.Contains("January", output);
        Assert.Contains("137.02", output);
        Assert.Contains("255.47", output);
        Assert.Contains("31", output);
        Assert.Contains("for 31 days", output);
    }

    [Fact]
    public void ReportGenerator_Should_Generate_Expected_Output_Jan2024_Feb_2024()
    {

        // Arrange: Load mock cost data from file
        var mockCostData = TestHelper.LoadMockCostData("mockCostData_jan2024-feb2024.json");

        // Arrange: Setup cost comparision context
        var comparisonReferenceDate = new DateTime(2024, 02, 10);
        var monthCalculationService = new MonthCalculationService();
        var monthComparisonPeriod = monthCalculationService.CalculateMonthComparisonPeriod(comparisonReferenceDate);
        var comparisonCalculation = new ComparisonCalculationService();

        var costComparisonContext = new CostComparisonContext(
            comparisonReferenceDate: comparisonReferenceDate,
            monthComparisonPeriod: monthComparisonPeriod,
            comparisonTableDayCount: comparisonCalculation.CalculateComparisonDayCount(comparisonReferenceDate),
            dataLoadDelayHours: 4
        );

        // Arrange: Initialize the report generator
        // we get this from base class - the service collection and report generator

        // Capture console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Act: Generate report
        base.ReportGenerator.GenerateDailyCostReport(mockCostData, costComparisonContext, showWeeklyPatterns: false, showDayOfWeekAverages: false);

        // Assert: Validate expected report output
        var output = consoleOutput.ToString();
        Assert.Contains("January", output);
        Assert.Contains("February", output);
        Assert.Contains("255.47", output);
        Assert.Contains("277.55", output);
        Assert.Contains("31", output);
        Assert.Contains("for 29 days", output);
    }

    [Fact]
    public void ReportGenerator_Should_Generate_Expected_Output_Feb_2024_Mar2024_Partial_Month()
    {

        // Arrange: Load mock cost data from file
        var mockCostData = TestHelper.LoadMockCostData("mockCostData_feb2024-mar2024.json");

        // Arrange: Setup cost comparision context
        var comparisonReferenceDate = new DateTime(2024, 03, 10);
        var monthCalculationService = new MonthCalculationService();
        var monthComparisonPeriod = monthCalculationService.CalculateMonthComparisonPeriod(comparisonReferenceDate);
        var comparisonCalculation = new ComparisonCalculationService();

        var costComparisonContext = new CostComparisonContext(
            comparisonReferenceDate: comparisonReferenceDate,
            monthComparisonPeriod: monthComparisonPeriod,
            comparisonTableDayCount: comparisonCalculation.CalculateComparisonDayCount(comparisonReferenceDate),
            dataLoadDelayHours: 4
        );

        // Arrange: Initialize the report generator
        // we get this from base class - the service collection and report generator

        // Capture console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Act: Generate report
        base.ReportGenerator.GenerateDailyCostReport(mockCostData, costComparisonContext, showWeeklyPatterns: false, showDayOfWeekAverages: false);

        // Assert: Validate expected report output
        var output = consoleOutput.ToString();
        Assert.Contains("February", output);
        Assert.Contains("March", output);
        Assert.Contains("277.55", output);
        Assert.Contains("617.95", output);
        Assert.Contains("31", output);
        Assert.Contains("29 days", output);
    }

    [Fact]
    public void ReportGenerator_Should_Generate_Expected_Output_CurrentDate()
    {

        // Arrange: Create dynamic mock data
        var mockCostData = CurrentDateDynamicCostDataCreator.GenerateCostData();


        // Arrange: Setup cost comparision context
        var comparisonReferenceDate = DateTime.UtcNow;
        var monthCalculationService = new MonthCalculationService();
        var monthComparisonPeriod = monthCalculationService.CalculateMonthComparisonPeriod(comparisonReferenceDate);
        var comparisonCalculation = new ComparisonCalculationService();

        var costComparisonContext = new CostComparisonContext(
            comparisonReferenceDate: comparisonReferenceDate,
            monthComparisonPeriod: monthComparisonPeriod,
            comparisonTableDayCount: comparisonCalculation.CalculateComparisonDayCount(comparisonReferenceDate),
            dataLoadDelayHours: 4
        );

        // Arrange: Initialize the report generator
        // we get this from base class - the service collection and report generator

        // Capture console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Act: Generate report
        base.ReportGenerator.GenerateDailyCostReport(mockCostData, costComparisonContext, showWeeklyPatterns: false, showDayOfWeekAverages: false);

        // Assert: Validate expected report output
        var output = consoleOutput.ToString();
        Assert.Contains(comparisonReferenceDate.Month.ToString(), output);
        //Assert.Contains("March", output);
        //Assert.Contains("277.55", output);
        //Assert.Contains("617.95", output);
        //Assert.Contains("31", output);
        //Assert.Contains("29 days", output); 
    }

}
