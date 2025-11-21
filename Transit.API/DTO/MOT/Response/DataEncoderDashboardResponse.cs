using Transit.Domain.Models.MOT;

namespace Transit.Api.Contracts.MOT.Response;

public class DataEncoderDashboardResponse
{
    public int TotalCustomersCreated { get; set; }
    public int PendingCustomerApprovals { get; set; }
    public int TotalServicesCreated { get; set; }
    public int PendingServiceApprovals { get; set; }
    public int DraftServices { get; set; }
    public List<Customer> RecentCustomers { get; set; } = new();
    public List<Service> RecentServices { get; set; } = new();
}


