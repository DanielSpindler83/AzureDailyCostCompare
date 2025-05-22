namespace AzureDailyCostCompare;

public class ConfigurationValidationException : Exception
{
    public ConfigurationValidationException(string message) : base(message)
    {
    }
}