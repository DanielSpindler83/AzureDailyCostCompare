using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDailyCostCompare.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {

        var previousDayUtcDataLoadDelayHours = new PreviousDayUtcDataLoadDelayHoursUserSetting(configuration);
        services.AddSingleton(previousDayUtcDataLoadDelayHours);

        // Register business services
        services.AddScoped<AuthenticationService>();
        services.AddScoped<BillingService>();
        services.AddScoped<CostService>();
        services.AddScoped<CostComparisonBusinessHandler>();
        services.AddScoped<CostComparisonContext>();
        services.AddScoped<ApplicationUnifiedSettings>();
        services.AddScoped<CostComparisonDateService>();
        services.AddScoped<DataAvailabilityService>();
        services.AddScoped<MonthCalculationService>();
        services.AddScoped<ComparisonCalculationService>();

        services.AddReportingServices();

        services.AddHttpClient<BillingService>(client =>
        {
            client.BaseAddress = new Uri("https://management.azure.com/");
            client.Timeout = TimeSpan.FromMinutes(5);
            // Add any default headers here if needed
            //client.DefaultRequestHeaders.Add("User-Agent", "AzureDailyCostCompare");
        });

        services.AddHttpClient<CostService>(client =>
        {
            client.BaseAddress = new Uri("https://management.azure.com/");
            client.Timeout = TimeSpan.FromMinutes(10); // Longer timeout for cost queries
            //client.DefaultRequestHeaders.Add("User-Agent", "AzureDailyCostCompare/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }

    public static IServiceCollection AddReportingServices(this IServiceCollection services)
    {
        services.AddScoped<ReportGenerator>();
        services.AddScoped<CostDataProcessor>();
        services.AddScoped<IReportRenderer,SpectreReportRenderer>();
        services.AddScoped<DailyCostTableBuilder>();
        services.AddScoped<WeeklyAnalysisService>();

        return services;
    }
}