namespace AzureDailyCostCompare;

public class DateHelperService
{
    public DateTime StartDate = DateTime.UtcNow.AddMonths(-1).Date;
    public DateTime EndDate = DateTime.UtcNow.Date;


    public DateHelperService()
    {
    }
}
