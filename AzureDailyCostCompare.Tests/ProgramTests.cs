using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Domain;
using AzureDailyCostCompare.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

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
        Assert.Contains("29 days", output);
    }

    [Fact]
    public void ReportGenerator_Should_Generate_Expected_Output_Feb_2024_Mar2024()
    {

        // Arrange: Load mock cost data from file
        var mockCostData = TestHelper.LoadMockCostData("mockCostData_feb2024-mar2024.json");

        // Arrange: Setup cost comparision context
        var costComparisonContext = new CostComparisonContext(
            ReferenceDate: new DateTime(2024, 03, 19),
            ComparisonType: ComparisonType.FullMonth,
            CurrentMonthStart: new DateTime(2024, 3, 1),
            PreviousMonthStart: new DateTime(2024, 2, 1),
            CurrentMonthDayCount: 31, // March
            PreviousMonthDayCount: 29, // Feb in a leap year 
            ComparisonTableDayCount: 31, // Compare against 31 days
            DataLoadDelayHours: 4
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
    public void ReportGenerator_Should_Generate_Expected_Output_Feb_2024_Mar2024_Partial_Month()
    {

        // Arrange: Load mock cost data from file
        var mockCostData = TestHelper.LoadMockCostData("mockCostData_feb2024-mar2024.json");

        // Arrange: Setup cost comparision context
        var costComparisonContext = new CostComparisonContext(
            ReferenceDate: new DateTime(2024, 03, 30),
            ComparisonType: ComparisonType.PartialMonth,
            CurrentMonthStart: new DateTime(2024, 3, 1),
            PreviousMonthStart: new DateTime(2024, 2, 1),
            CurrentMonthDayCount: 31, // March
            PreviousMonthDayCount: 29, // Feb in a leap year 
            ComparisonTableDayCount: 31, // Compare against 31 days
            DataLoadDelayHours: 4
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



    // test cases needed or unit test the logic for this

    /*
     
    current month long and previous month short:
    March 31 days current and Feb as previous month(in a leap year and not in a leap year?) maybe use 2024 feb (leap year)

    current month short and previous month long:
    Feb 2025 28 days as current and January as previous month 31 days

    ensure we get expected output in both above scenarios


    I NEED PARTIAL MONTH tests for when its using the current month as the actualy current month to see how it looks
    currently historical will show full month all the time and thats cool - maybe need to cater for that as is but this month we are in will be different.

    yeah i dont think i can easily mock the current month - i do need to be able to mock currentdatetimeinUTC so i can run mock data for the current parital month

    */

}
