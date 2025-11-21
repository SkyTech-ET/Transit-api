using Transit.Domain.Models.MOT;

namespace Transit.Application;

public class ApproveCustomerCommand : IRequest<OperationResult<Customer>>
{
    public long CustomerId { get; set; }
    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
    public long VerifiedByUserId { get; set; }
}




