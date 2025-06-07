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
        var costComparisonContext = new CostComparisonContext(
            comparisonReferenceDate: new DateTime(2024, 01, 01),
            comparisonType: ComparisonType.FullMonth,
            currentMonthStart: new DateTime(2024, 1, 1),   // January 1st
            previousMonthStart: new DateTime(2023, 12, 1), // December 1st
            currentMonthDayCount: 31,  // Only 1 day in current month (Jan 1st)
            previousMonthDayCount: 31, // Full December (31 days)
            comparisonTableDayCount: 31, // Compare against 31 days
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
        var costComparisonContext = new CostComparisonContext(
            comparisonReferenceDate: new DateTime(2024, 02, 10),
            comparisonType: ComparisonType.FullMonth,
            currentMonthStart: new DateTime(2024, 2, 1),   
            previousMonthStart: new DateTime(2024, 1, 1), 
            currentMonthDayCount: 29, // Feb in a leap year  
            previousMonthDayCount: 31, // Jan
            comparisonTableDayCount: 31, // Compare against 31 days
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
        var costComparisonContext = new CostComparisonContext(
            comparisonReferenceDate: new DateTime(2024, 03, 31),
            comparisonType: ComparisonType.PartialMonth,
            currentMonthStart: new DateTime(2024, 3, 1),
            previousMonthStart: new DateTime(2024, 2, 1),
            currentMonthDayCount: 31, // March
            previousMonthDayCount: 29, // Feb in a leap year 
            comparisonTableDayCount: 31, // Compare against 31 days
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
        Assert.Contains("29 days", output); // failing here as we show Feb has 31 days.....which is WRONG
    }

    [Fact]
    public void ReportGenerator_Should_Generate_Expected_Output_CurrentDate()
    {

        // Arrange: Create dynamic mock data
        var mockCostData = CurrentDateDynamicCostDataCreator.GenerateCostData();

        var referenceDate = DateTime.UtcNow;

        var previousMonthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1).AddMonths(-1);

        // Arrange: Setup cost comparision context
        var costComparisonContext = new CostComparisonContext(
            comparisonReferenceDate: referenceDate,
            comparisonType: ComparisonType.PartialMonth,
            currentMonthStart: new DateTime(referenceDate.Year, referenceDate.Month, 1),
            previousMonthStart: previousMonthStart,
            currentMonthDayCount: DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month),
            previousMonthDayCount: DateTime.DaysInMonth(previousMonthStart.Year, previousMonthStart.Month),
            comparisonTableDayCount: referenceDate.Day,
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
        Assert.Contains(referenceDate.Month.ToString(), output);
        //Assert.Contains("March", output);
        //Assert.Contains("277.55", output);
        //Assert.Contains("617.95", output);
        //Assert.Contains("31", output);
        //Assert.Contains("29 days", output); 
    }

}
