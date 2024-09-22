
namespace AzureDailyCostCompare;

class ReportGenerator
{
    public void GenerateDailyCostReport(List<DailyCosts> costData)
    {
        Console.WriteLine("Daily Cost Report:");
        foreach (var dailyCost in costData)
        {
            Console.WriteLine($"{dailyCost.DateString}: {dailyCost.Cost:C}");
        }
    }
}