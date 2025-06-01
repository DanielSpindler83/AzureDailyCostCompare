using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Tests;

public class ProgramTests
{
    [Fact]
    public void ReportGenerator_Should_Generate_Expected_Output_Dec2023_Jan2024()
    {
        // Arrange: Load mock cost data from file
        var mockCostData = TestHelper.LoadMockCostData("mockCostData_dec2023-jan2024.json");

        // Arrange: Setup cost comparision context
        var costComparisonContext = new CostComparisonContext(
            ReferenceDate: new DateTime(2024, 01, 01),
            ComparisonType: ComparisonType.FullMonth,
            CurrentMonthStart: new DateTime(2024, 1, 1),   // January 1st
            PreviousMonthStart: new DateTime(2023, 12, 1), // December 1st
            CurrentMonthDayCount: 31,  // Only 1 day in current month (Jan 1st)
            PreviousMonthDayCount: 31, // Full December (31 days)
            ComparisonTableDayCount: 31, // Compare against 31 days
            DataLoadDelayHours: 4
        );

        // Arrange: Initialize the report generator
        var reportGenerator = new ReportGenerator();

        // Capture console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Act: Generate report
        reportGenerator.GenerateDailyCostReport(mockCostData, costComparisonContext, showWeeklyPatterns: false, showDayOfWeekAverages: false);

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
            ReferenceDate: new DateTime(2024, 02, 10),
            ComparisonType: ComparisonType.FullMonth,
            CurrentMonthStart: new DateTime(2024, 2, 1),   
            PreviousMonthStart: new DateTime(2024, 1, 1), 
            CurrentMonthDayCount: 29, // Feb in a leap year  
            PreviousMonthDayCount: 31, // Jan
            ComparisonTableDayCount: 31, // Compare against 31 days
            DataLoadDelayHours: 4
        );

        // Arrange: Initialize the report generator
        var reportGenerator = new ReportGenerator();

        // Capture console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Act: Generate report
        reportGenerator.GenerateDailyCostReport(mockCostData, costComparisonContext, showWeeklyPatterns: false, showDayOfWeekAverages: false);

        // Assert: Validate expected report output
        var output = consoleOutput.ToString();
        Assert.Contains("January", output);
        Assert.Contains("February", output);
        Assert.Contains("255.47", output);
        Assert.Contains("277.55", output);
        Assert.Contains("31", output);
        Assert.Contains("for 29 days", output);
    }
}
