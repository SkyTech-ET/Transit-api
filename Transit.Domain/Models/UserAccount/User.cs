namespace Transit.Domain.Models;

public class User : BaseEntity
{
    private readonly List<UserRole> _roles = new List<UserRole>();
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string ProfilePhoto { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public bool IsSuperAdmin { get; private set; }
    public bool IsAccountLocked { get; private set; }
    public string VerificationToken { get; private set; } = string.Empty;
    public int LoginAttemptCount { get; private set; }
    public DateTime LastLoginDateTime { get; private set; }
    public bool IsConfirmationEmailSent { get; private set; }
    public int userTokenLifetime { get; private set; } = 7_200;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpireDate { get; set; }
    public ICollection<UserRole> UserRoles { get { return _roles; } }
    // ✅ One-to-Many: A User belongs to one Store
    public static User CreateUser(
          string userName,
          string email,
          string firstName,
          string lastName,
          string profilePhoto,
          string phone,
          string password,
          bool isSuperAdmin,
          AccountStatus accountStatus
          )
    {
        return new User
        {
            Username = userName,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            ProfilePhoto = profilePhoto,
            Phone = phone,
            Password = password,
            IsSuperAdmin = isSuperAdmin,
            RecordStatus = Shared.RecordStatus.Active,
            AccountStatus = AccountStatus.Approved,
        };
    }
    public void UpdateUser(
        string firstName,
        string lastName,
        string profilePhoto,
        string phone,
        bool isSuperAdmin,
        string UserName,
        string Email,
        RecordStatus recordStatus,
        AccountStatus accountStatus
        )
    {
        this.FirstName = firstName;
        this.LastName = lastName;
        this.ProfilePhoto = profilePhoto;
        this.Phone = phone;
        this.IsSuperAdmin = isSuperAdmin;
        this.Username = UserName;
        this.Email = Email;
        this.RecordStatus = recordStatus;
        this.AccountStatus = accountStatus;
    }
    public void UpdatePassword(string passsword)
    {
        Password = passsword;
    }

    public void AddRole(UserRole role)
    {
        _roles.Add(role);
    }
}
