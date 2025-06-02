using Microsoft.Extensions.Configuration;

namespace AzureDailyCostCompare.Infrastructure;

public class PreviousDayUtcDataLoadDelayHoursUserSetting
{
    public PreviousDayUtcDataLoadDelayHoursUserSetting(IConfiguration configuration)
    {
        configuration.GetSection(nameof(PreviousDayUtcDataLoadDelayHoursUserSetting)).Bind(this);

        if (!NumberOfHours.HasValue)
        {
            throw new InvalidOperationException(
                $"{nameof(PreviousDayUtcDataLoadDelayHoursUserSetting)}.{nameof(NumberOfHours)}  configuration is missing or invalid. Please specify a valid number between 0 and 23.");
        }

        if (NumberOfHours < 0 || NumberOfHours > 23)
        {
            throw new InvalidOperationException(
                $"{nameof(PreviousDayUtcDataLoadDelayHoursUserSetting)}.{nameof(NumberOfHours)}  must be between 0 and 23. Please check configuration.");
        }
    }

    public int? NumberOfHours { get; set; }
}
