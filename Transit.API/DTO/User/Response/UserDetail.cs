using Transit.Domain.Models.Shared;

namespace Transit.Api.Contracts.User.Response;

public class UserDetail
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string ProfilePhoto { get; set; }
    public RecordStatus RecordStatus { get; set; }

    public string LastName { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsAccountLocked { get; set; }
    public string PhoneNumber { get; set; }
    public string VerificationToken { get; set; }
    public int LoginAttemptCount { get; set; }
    // 🔹 Change from long to List<long> to support multiple store IDs
    public List<long> StoreIds { get; set; } = new List<long>();
    public DateTime LastLoginDateTime { get; set; }
    public bool IsConfirmationEmailSent { get; set; }
    public List<RoleDetail> Roles { get; set; }
    public StoreDetail UserStore { get; set; }
}


