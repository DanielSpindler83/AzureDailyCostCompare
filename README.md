
# AzureDailyCostCompare

## Overview

AzureDailyCostCompare is a tool designed to help you compare daily costs for Azure services across a specified billing scope between two consecutive months. The application provides a detailed report that highlights full-day cost differences by day and calculates running monthly averages, helping organizations monitor and understand cloud spending trends within their overarching billing context.


## Sample Report

```
Day of Month       December           January            Cost Difference(USD)
1                  2800.89            2806.26            5.37
2                  1852.19            1834.37            -17.82
3                  1850.99            1830.96            -20.03
4                  1861.91            1814.77            -47.14
5                  1882.82            1834.32            -48.50
6                  1865.77            1872.07            6.30
7                  1831.34            1882.61            51.27
8                  1843.22            1873.93            30.71

Monthly Cost Averages                                                  Amount (USD)
----------------------------------------------------------------------------------
January average (for 8 days)                                             1834.91
December average (for 8 days)                                            1852.85
Month averages cost delta (January average minus December average)        -17.94
December full month average                                               2029.78

------ Data Reference Information ------
All costs in USD
A day's data is considered complete 4 hours after the end of the day in UTC time.
Daily cost data is complete up to end of the day 2025-01-08 in UTC timezone
The end of the day in UTC time is 08/01/2025 10:00:00 AM in local timezone of (UTC+10:00) Brisbane

This report was generated at 08/01/2025 6:17:17 PM (UTC+10:00) Brisbane
This report was generated at 08/01/2025 8:17:17 AM UTC

```

## Features


| Feature | Description |
|---------|-------------|
| **Billing Account Scope** | Compare costs across Azure subscriptions and Azure tenants within a single Azure billing account. |
| **Daily Cost Comparison** | Compare the costs for each day between two months. |
| **Monthly Average Analysis** | Calculate and display the monthly cost averages. |
| **Cost Delta Calculation** | Highlight the difference in costs between the months. |
| **Flexible Date Handling** | Generate reports for any specified reference date. |
| **Weekly Pattern Analysis** | Ability to compare corresponding weeks within the month (1st Monday to 1st Monday, etc.) |
| **Day of Week Averages** | View the average for each day of the week in the month (All Mondays, Tuesdays, etc) |

## Weekly Pattern Analysis
Run via a command line option --day-of-week-averages or -dowa

```
------ Weekly Pattern Analysis (UTC) ------
Comparing corresponding weeks (1st Monday to 1st Monday, etc.)

Sundays:
First    Apr 06 (UTC):   1890.32 -> May 04 (UTC):   1875.47 =     -14.85
Second   Apr 13 (UTC):   1941.98 -> May 11 (UTC):   1892.71 =     -49.27
Third    Apr 20 (UTC):   1882.91 -> May 18 (UTC):   1886.47 =      +3.56

Mondays:
First    Apr 07 (UTC):   1930.60 -> May 05 (UTC):   1915.95 =     -14.65
Second   Apr 14 (UTC):   1926.43 -> May 12 (UTC):   1928.22 =      +1.79
Third    Apr 21 (UTC):   1911.20 -> May 19 (UTC):   1918.99 =      +7.79

Tuesdays:
First    Apr 01 (UTC):   2858.22 -> May 06 (UTC):   1921.89 =    -936.33
Second   Apr 08 (UTC):   1923.13 -> May 13 (UTC):   1981.82 =     +58.69
Third    Apr 15 (UTC):   1924.13 -> May 20 (UTC):   1921.05 =      -3.08

Wednesdays:
First    Apr 02 (UTC):   1915.30 -> May 07 (UTC):   1931.69 =     +16.39
Second   Apr 09 (UTC):   1924.23 -> May 14 (UTC):   1925.70 =      +1.47
Third    Apr 16 (UTC):   2708.75 -> May 21 (UTC):   1921.78 =    -786.97

Thursdays:
First    Apr 03 (UTC):   1933.09 -> May 01 (UTC):   2826.03 =    +892.94
Second   Apr 10 (UTC):   2003.73 -> May 08 (UTC):   1925.28 =     -78.45
Third    Apr 17 (UTC):   3107.55 -> May 15 (UTC):   1928.22 =   -1179.33
Fourth   Apr 24 (UTC):   1918.34 -> May 22 (UTC):   2000.19 =     +81.85

Fridays:
First    Apr 04 (UTC):   1930.20 -> May 02 (UTC):   1873.63 =     -56.57
Second   Apr 11 (UTC):   1923.98 -> May 09 (UTC):   1917.45 =      -6.53
Third    Apr 18 (UTC):   1900.46 -> May 16 (UTC):   2690.66 =    +790.20

Saturdays:
First    Apr 05 (UTC):   1904.11 -> May 03 (UTC):   1855.06 =     -49.05
Second   Apr 12 (UTC):   1881.05 -> May 10 (UTC):   1964.48 =     +83.43
Third    Apr 19 (UTC):   1878.79 -> May 17 (UTC):   2993.15 =   +1114.36
```

## Day of Week Averages
Run via a command line option --weekly-pattern-analysis or -wpa

```
------ Day of Week Averages (UTC) ------
Day            Prev Avg     Curr Avg         Diff  Samples
-------------------------------------------------------
Sunday          1900.07      1884.88       -15.18    (4/3)
Monday          1926.90      1921.05        -5.85    (4/3)
Tuesday         2135.75      1941.59      -194.16    (5/3)
Wednesday       2115.68      1926.39      -189.29    (5/3)
Thursday        2240.68      2169.93       -70.75    (4/4)
Friday          2056.91      2160.58      +103.67    (4/3)
Saturday        2094.83      2270.90      +176.06    (4/3)
```

## Requirements

- Dotnet Core Runtime 8 or higher
- Azure CLI (used for authentication)
- Azure Cost Management Permissions at Billing Account Scope

## Azure Permissions
**Needs testing\confirmation**

Grant the account you wish to use either:

**Reader** or **Billing Reader** or **Owner** role in Azure

within Azure **Cost Management + Billing** - **Access control (IAM)**

## Installation

1. Install .NET Runtime 8 or higher

	Install .NET Runtime - The .NET Runtime contains just the components needed to run a console app.
	https://dotnet.microsoft.com/en-us/download/dotnet
	

2. Install Azure CLI:

	https://learn.microsoft.com/en-us/cli/azure/install-azure-cli
   
3. Authenticate using Azure CLI:
   ```bash
   az login
   ```
   https://learn.microsoft.com/en-us/cli/azure/authenticate-azure-cli-interactively
   
4. Install via dotnet tool command
	```bash
	dotnet tool install --global azure-daily-cost-compare
	```

## Upgrading

When there is a new version available on NuGet, you can use the `dotnet tool update` command to upgrade:

```bash
dotnet tool update --global azure-daily-cost-compare
```


## Usage

### Command-Line Options

```
Description:
  Azure Daily Cost Comparison Tool

Usage:
  azure-daily-cost-compare [options]

Options:
  --date <yyyy-MM-dd>                                  Optional reference date for the report (format: yyyy-MM-dd). If not provided, current date will be used.
  -wpa, --weekly-pattern-analysis                      Show weekly pattern analysis comparing corresponding weekdays
  -dowa, --day-of-week-averages                        Show day of week averages comparing cost trends by weekday
  -pdl, --previous-day-utc-data-load-delay <int:0-23>  Number of hours after midnight UTC used to determine when the previous day's Azure cost data is considered complete enough to load. For
                                                       example, a value of 4 means data for the previous day is assumed complete at 04:00 UTC. Valid values: 0-23.
  --version                                            Show version information
  -?, -h, --help                                       Show help and usage information
```

### Example Command

```
azure-daily-cost-compare
```

This command generates a cost comparison report using today as the reference date.

### Example Command with Specific Date

```bash
azure-daily-cost-compare --date 2025-01-28
```

This command generates a cost comparison report using 2025-01-28 as the reference date.

### Example command to set previous day UTC data load delay value

By default cost data is considered complete 4 hours after the end of the day in UTC.

You can configure this via the --previous-day-utc-data-load-delay command line option.

Setting value will be used for the current execution and persist for future executions as it is saved in the appsettings.json config file.

```
azure-daily-cost-compare --previous-day-utc-data-load-delay 8
```

## Data Accuracy

Please note that Microsoft Cost Management data is refreshed every four hours as new usage data is received from Azure resource providers.

https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/manage-automation#data-latency-and-rate-limits

Cost data may change later on as Microsoft reviews and updates pricing information in the background prior to official invoicing. Please keep this in mind.


## License

This project is licensed under the Apache-2.0 license.
