namespace Transit.Api.Contracts.MOT.Request;

public class CustomerApprovalRequest
{
    public long CustomerId { get; set; }
    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
}

