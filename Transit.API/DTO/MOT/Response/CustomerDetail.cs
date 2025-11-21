namespace Transit.Api.Contracts.MOT.Response;

public class CustomerDetail
{
    public long Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string TINNumber { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public long? VerifiedByUserId { get; set; }
    public DateTime RegisteredDate { get; set; }
}


