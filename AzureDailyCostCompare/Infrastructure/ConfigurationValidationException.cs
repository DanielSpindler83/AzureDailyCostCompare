namespace AzureDailyCostCompare.Infrastructure;

public class ConfigurationValidationException : Exception
{
    public ConfigurationValidationException(string message) : base(message)
    {
    }
}