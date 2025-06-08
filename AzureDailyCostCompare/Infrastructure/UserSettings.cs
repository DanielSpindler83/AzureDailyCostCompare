using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text.Json;

namespace AzureDailyCostCompare.Infrastructure;

//public class UserSettings
//{
//    public UserSettings(IConfiguration configuration)
//    {
//        PreviousDayUtcDataLoadDelayHours = new PreviousDayUtcDataLoadDelayHoursUserSetting(configuration);
//    }

//    public PreviousDayUtcDataLoadDelayHoursUserSetting PreviousDayUtcDataLoadDelayHours { get; }
//}


public class UserSettings
{
    private const string APP_NAME = "azure-daily-cost-compare"; 

    public UserSettings(IConfiguration configuration)
    {
        PreviousDayUtcDataLoadDelayHours = new PreviousDayUtcDataLoadDelayHoursUserSetting(configuration);
    }

    public PreviousDayUtcDataLoadDelayHoursUserSetting PreviousDayUtcDataLoadDelayHours { get; }

    // Static helper methods for settings file management
    public static string GetUserSettingsPath()
    {
        string settingsDir;

        if (OperatingSystem.IsWindows())
        {
            var userAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            settingsDir = Path.Combine(userAppData, APP_NAME);
        }
        else if (OperatingSystem.IsMacOS())
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            settingsDir = Path.Combine(homeDir, "Library", "Application Support", APP_NAME);
        }
        else
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var configDir = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
                           ?? Path.Combine(homeDir, ".config");
            settingsDir = Path.Combine(configDir, APP_NAME);
        }

        Directory.CreateDirectory(settingsDir);
        return Path.Combine(settingsDir, "usersettings.json");
    }

    // Get the default settings file path from the app directory
    private static string GetDefaultSettingsPath()
    {
        var appDir = AppContext.BaseDirectory;
        return Path.Combine(appDir, "usersettings.json");
    }

    public static IConfiguration CreateUserConfiguration()
    {
        var userSettingsPath = GetUserSettingsPath();

        // If user settings don't exist, copy from app directory
        if (!File.Exists(userSettingsPath))
        {
            var defaultSettingsPath = GetDefaultSettingsPath();
            File.Copy(defaultSettingsPath, userSettingsPath);
        }

        var configBuilder = new ConfigurationBuilder();

        // Only load from user settings directory
        if (File.Exists(userSettingsPath))
        {
            configBuilder.AddJsonFile(userSettingsPath, optional: true, reloadOnChange: true);
        }

        return configBuilder.Build();
    }

    // Save current settings to user settings file
    public static async Task SaveUserSettingsAsync(UserSettings settings)
    {
        var settingsPath = GetUserSettingsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));

        // Create settings object dynamically based on actual settings
        var settingsObject = CreateSettingsObject(settings);

        var json = JsonSerializer.Serialize(settingsObject, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(settingsPath, json);
    }

    // Dynamically create settings object from UserSettings instance
    private static object CreateSettingsObject(UserSettings settings)
    {
        var settingsDict = new Dictionary<string, object>();

        // Use reflection to get all properties and their values
        var properties = typeof(UserSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(settings);
            if (value != null)
            {
                // For complex settings objects, create a dictionary of their properties
                var settingDict = new Dictionary<string, object>();
                var settingProperties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var settingProp in settingProperties)
                {
                    var settingValue = settingProp.GetValue(value);
                    if (settingValue != null)
                    {
                        settingDict[settingProp.Name] = settingValue;
                    }
                }

                settingsDict[prop.Name + "UserSetting"] = settingDict;
            }
        }

        return settingsDict;
    }

    // Convenience method to save just when a specific setting changes
    public static void SaveSetting<T>(string settingName, T settingValue)
    {
        var settingsPath = GetUserSettingsPath();

        Dictionary<string, object> existingSettings = new();

        // Load existing settings if file exists
        if (File.Exists(settingsPath))
        {
            var existingJson = File.ReadAllText(settingsPath);
            existingSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(existingJson) ?? new();
        }

        // Update the specific setting
        existingSettings[settingName] = settingValue;

        var json = JsonSerializer.Serialize(existingSettings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(settingsPath, json);
    }
}