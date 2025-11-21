namespace Transit.Api.Contracts.MOT.Request;

public class VerifyDocumentRequest
{
    public long DocumentId { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationNotes { get; set; }
}


