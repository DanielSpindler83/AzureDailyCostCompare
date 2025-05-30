using Microsoft.Extensions.Configuration;

namespace AzureDailyCostCompare.Infrastructure;

public class PreviousDayUtcDataLoadDelayHours
{
    public PreviousDayUtcDataLoadDelayHours(IConfiguration configuration)
    {
        configuration.GetSection(nameof(PreviousDayUtcDataLoadDelayHours)).Bind(this);

        if (!NumberOfHours.HasValue)
        {
            throw new InvalidOperationException(
                $"{nameof(PreviousDayUtcDataLoadDelayHours)} configuration is missing or invalid. Please specify a valid number between 0 and 23.");
        }

        if (NumberOfHours < 0 || NumberOfHours > 23)
        {
            throw new InvalidOperationException(
                $"{nameof(PreviousDayUtcDataLoadDelayHours)} must be between 0 and 23. Please check configuration.");
        }
    }

    public int? NumberOfHours { get; set; }
}
