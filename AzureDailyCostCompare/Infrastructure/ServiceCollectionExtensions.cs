using AzureDailyCostCompare.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDailyCostCompare.Infrastructure;

public static class ServiceCollectionExtensions
{
    public const string ConfigFileName = "appsettings.json";

    public static ServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(ConfigFileName, optional: false, reloadOnChange: false)
            .Build();


        var services = new ServiceCollection();

        services.ConfigureServices(configuration);

        return services.BuildServiceProvider();
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {

        // services.AddSingleton<IConfiguration>(configuration); NEEDED OR NOT?
        var previousDayUtcDataLoadDelayHours = new PreviousDayUtcDataLoadDelayHours(configuration);
        services.AddSingleton(previousDayUtcDataLoadDelayHours);
        // so now i can simply inject previousDayUtcDataLoadDelayHours and use it where it is needed?

        services.AddScoped<ReportGeneratorFactory>();


        // Register HTTP client
        services.AddHttpClient();

        // Register your business services
        services.AddScoped<AuthenticationService>();
        services.AddScoped<BillingService>();
        services.AddScoped<CostService>();
        services.AddScoped<CostComparisonBusinessHandler>();

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
}