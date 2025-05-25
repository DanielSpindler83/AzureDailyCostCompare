using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Application.Interfacces;
using AzureDailyCostCompare.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDailyCostCompare.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.ConfigureServices();

        return services.BuildServiceProvider();
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Register HTTP client
        services.AddHttpClient();

        // Register your business services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<ICostService, CostService>();
        services.AddScoped<ICostComparisonHandler, CostComparisonBusinessHandler>();

        services.AddHttpClient<IBillingService, BillingService>(client =>
        {
            client.BaseAddress = new Uri("https://management.azure.com/");
            client.Timeout = TimeSpan.FromMinutes(5);
            // Add any default headers here if needed
            //client.DefaultRequestHeaders.Add("User-Agent", "AzureDailyCostCompare");
        });

        services.AddHttpClient<ICostService, CostService>(client =>
        {
            client.BaseAddress = new Uri("https://management.azure.com/");
            client.Timeout = TimeSpan.FromMinutes(10); // Longer timeout for cost queries
            //client.DefaultRequestHeaders.Add("User-Agent", "AzureDailyCostCompare/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}