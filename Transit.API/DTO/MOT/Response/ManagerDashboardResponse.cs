using Transit.Domain.Models.MOT;

namespace Transit.Api.Contracts.MOT.Response;

public class ManagerDashboardResponse
{
    public int TotalServices { get; set; }
    public int PendingServices { get; set; }
    public int InProgressServices { get; set; }
    public int CompletedServices { get; set; }
    public int TotalCustomers { get; set; }
    public int VerifiedCustomers { get; set; }
    public int TotalStaff { get; set; }
    public int ActiveStaff { get; set; }
    public List<Service> RecentServices { get; set; } = new();
    public List<MonthlyServiceStat> MonthlyServiceStats { get; set; } = new();
}

public class MonthlyServiceStat
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalServices { get; set; }
    public int CompletedServices { get; set; }
    public double CompletionRate { get; set; }
}


