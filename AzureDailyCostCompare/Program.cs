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

            // Build command structure (command execution delegates to handler that pulls from DI)
            var rootCommand = CommandLineBuilder.BuildCommandLine(serviceProvider); 
            //is it ok to pass in service provider? I thought it better than globally store ServiceProviderAccessor.ServiceProvider = serviceProvider; ???

            // System.CommandLine framework executes the registered handler
            return await rootCommand.InvokeAsync(args);
        }
        catch (ConfigurationValidationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Error code 2: Configuration error: {ex.Message}");
            Console.ResetColor();
            return 2;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Error code 1: Unexpected error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }
}



