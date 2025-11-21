using Transit.Domain.Models.MOT;

namespace Transit.Api.Contracts.MOT.Response;

public class AssessorDashboardResponse
{
    public int PendingCustomerApprovals { get; set; }
    public int PendingServiceReviews { get; set; }
    public int ServicesUnderOversight { get; set; }
    public int CompletedReviewsToday { get; set; }
    public List<Customer> RecentCustomerApprovals { get; set; } = new();
    public List<Service> RecentServiceReviews { get; set; } = new();
}


