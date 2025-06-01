using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AzureDailyCostCompare.Tests;

public abstract class ReportGeneratorTestBase : IDisposable
{
    protected ServiceProvider ServiceProvider { get; }
    protected ReportGenerator ReportGenerator { get; }

    protected ReportGeneratorTestBase()
    {
        var services = new ServiceCollection();
        services.AddReportingServices();

        // For tests, you might want to use the console renderer instead of Spectre
        services.Replace(ServiceDescriptor.Scoped<IReportRenderer, ConsoleReportRenderer>());

        ServiceProvider = services.BuildServiceProvider();
        ReportGenerator = ServiceProvider.GetRequiredService<ReportGenerator>();
    }

    public void Dispose()
    {
        ServiceProvider?.Dispose();
    }
}
