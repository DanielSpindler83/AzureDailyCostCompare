using AzureDailyCostCompare.Infrastructure;
using System.CommandLine;

namespace AzureDailyCostCompare;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Configure DI container
            var serviceProvider = ServiceCollectionExtensions.BuildServiceProvider();

            // Store service provider globally so handlers can access it
            ServiceProviderAccessor.ServiceProvider = serviceProvider;

            // Build command structure (no execution logic here)
            var rootCommand = CommandLineBuilder.BuildCommandLine();

            // System.CommandLine framework executes the registered handler
            return await rootCommand.InvokeAsync(args);
        }
        catch (ConfigurationValidationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Configuration error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
        finally
        {
            // Clean up DI container
            ServiceProviderAccessor.ServiceProvider?.Dispose();
        }
    }
}