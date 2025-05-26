

using AzureDailyCostCompare.Application;

namespace AzureDailyCostCompare.Tests;

public class ProgramTests
{
    [Fact]
    public void ReportGenerator_Should_Generate_Expected_Output_Dec2023_Jan2024()
    {
        // Arrange: Load mock cost data from file
        var mockCostData = TestHelper.LoadMockCostData("mockCostData_dec2023-jan2024.json");

        // Arrange: Use a fixed reference date
        var referenceDate = new DateTime(2024, 01, 01);
        var cutoffHourUtc = 4;
        var costComparisonDateService = new CostComparisonDateService(cutoffHourUtc, referenceDate);

        // Arrange: Initialize the report generator
        var reportGenerator = new ReportGenerator(mockCostData, costComparisonDateService);

        // Capture console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Act: Generate report
        reportGenerator.GenerateDailyCostReport(showWeeklyPatterns: false, showDayOfWeekAverages: false);

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

        // Arrange: Use a fixed reference date
        var referenceDate = new DateTime(2024, 02, 10);
        var cutoffHourUtc = 4;
        var CostComparisonDateService = new CostComparisonDateService(cutoffHourUtc, referenceDate);

        // Arrange: Initialize the report generator
        var reportGenerator = new ReportGenerator(mockCostData, CostComparisonDateService);

        // Capture console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Act: Generate report
        reportGenerator.GenerateDailyCostReport(showWeeklyPatterns: false, showDayOfWeekAverages: false);

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
