using Microsoft.Extensions.Configuration;

namespace AzureDailyCostCompare.Infrastructure;

public class UserSettings
{
    public UserSettings(IConfiguration configuration)
    {
        PreviousDayUtcDataLoadDelayHours = new PreviousDayUtcDataLoadDelayHoursUserSetting(configuration);
    }

    public PreviousDayUtcDataLoadDelayHoursUserSetting PreviousDayUtcDataLoadDelayHours { get; }
}