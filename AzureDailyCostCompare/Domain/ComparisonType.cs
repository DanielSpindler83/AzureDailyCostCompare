namespace AzureDailyCostCompare.Domain;

/// <summary>
/// Type of comparison being performed
/// DOMAIN: Business concept representing different comparison strategies
/// </summary>
public enum ComparisonType
{
    /// <summary>Current month partial comparison (month-to-date)</summary>
    PartialMonth,
    /// <summary>Historical full month comparison</summary>
    FullMonth
}