
# AzureDailyCostCompare

## Overview

AzureDailyCostCompare is a tool designed to help you compare daily costs for Azure services across a specified billing scope between two consecutive months. The application provides a detailed report that highlights full-day cost differences by day and calculates running monthly averages, helping organizations monitor and understand cloud spending trends within their overarching billing context.


## Sample Report

```
Day of Month       December           January            Cost Difference(USD)
1                  2800.89            2806.26            5.37
2                  1852.19            1834.37            -17.82
3                  1850.99            1830.96            -20.03
...
Monthly Cost Averages                                                  Amount (USD)
----------------------------------------------------------------------------------
January average (for 28 days)                                             2042.37
December average (for 28 days)                                            2029.78
Month averages cost delta (January average minus December average)          12.59
December full month average                                               2029.78

------ Data Reference Information ------
All costs in USD
A day's data is considered complete 4 hours after the end of the day in UTC time.
Daily cost data is complete up to end of the day 2025-01-28 in UTC timezone.
The end of the day in UTC time is 28/01/2025 10:00:00 AM in local timezone of (UTC+10:00) Brisbane

This report was generated at 29/01/2025 6:17:17 PM (UTC+10:00) Brisbane
This report was generated at 29/01/2025 8:17:17 AM UTC
```

## Features

- **Billing Account Scope:** Compare costs across subscriptions within a single billing account.
- **Daily Cost Comparison:** Compare the costs for each day between two months.
- **Monthly Average Analysis:** Calculate and display the monthly cost averages.
- **Cost Delta Calculation:** Highlight the difference in costs between the months.
- **Flexible Date Handling:** Generate reports for any specified reference date.

## Requirements

- Dotnet Core Runtime 8 or higher
- Azure CLI (used for authentication)
- Azure Cost Management Permissions at Billing Account Scope

## Azure Permissions
**Needs testing\confirmation**

Grant the account you wish to use either:

**Reader** or **Billing Reader** role 

via Azure **Cost Management + Billing** - **Access control (IAM)**

## Installation

1. Install Azure CLI:

	https://learn.microsoft.com/en-us/cli/azure/install-azure-cli
   
2. Authenticate using Azure CLI:
   ```bash
   az login
   ```
   https://learn.microsoft.com/en-us/cli/azure/authenticate-azure-cli-interactively

3. Clone this repository:
   ```bash
   git clone https://github.com/DanielSpindler83/AzureDailyCostCompare.git
   ```
4. Navigate to the project directory:
   ```bash
   cd AzureDailyCostCompare
   ```
5. Build the project:
   ```bash
   dotnet build
   ```
6. Run the application:
   ```bash
   dotnet run
   ```

## Usage

### Command-Line Options

```
Description:
  Azure Daily Cost Comparison Tool

Usage:
  AzureDailyCostCompare [options]

Options:
  --date <date>   Optional reference date for the report (format: yyyy-MM-dd). If not provided, the current date will be used.
  --version       Show version information
  -?, -h, --help  Show help and usage information
```

### Example Command

```bash
AzureDailyCostCompare
```

This command generates a cost comparison report using today as the reference date.

### Example Command with Specific Date

```bash
AzureDailyCostCompare --date 2025-01-28
```

This command generates a cost comparison report using 2025-01-28 as the reference date.

## Notes

- **Data Accuracy:** Cost data is considered complete 4 hours after the end of the day in UTC.
- **Data Completeness:** Ensure Azure cost data is fully available before generating the report.

## License

This project is licensed under the Apache-2.0 license.
