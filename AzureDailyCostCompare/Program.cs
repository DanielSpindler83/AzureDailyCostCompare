using System.CommandLine;
using Microsoft.Extensions.Configuration;

namespace AzureDailyCostCompare;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var configService = new ConfigurationService();
            IConfiguration configuration = configService.LoadConfiguration();
            
            configService.ValidatePreviousDayUtcDataLoadDelayHours(configuration);

            int previousDayUtcDataLoadDelayHours = configuration.GetValue<int>("AppSettings:PreviousDayUtcDataLoadDelayHours:Value");

            var rootCommand = CommandLineBuilder.BuildCommandLine(previousDayUtcDataLoadDelayHours, configService);
            return await rootCommand.InvokeAsync(args);
        }
        catch (ConfigurationValidationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Configuration error: {ex.Message}");
            Console.ResetColor();
            return 1; // Return non-zero exit code to indicate error
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }
}