namespace Transit.Domain;

public class UserRole : BaseEntity
{
    public long? UserId { get; set; }
    public long? RoleId { get; set; }
    public virtual User User { get; set; }
    public virtual Role Role { get; set; }

    public static UserRole Create(long userId, long roleId)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            RecordStatus = Models.Shared.RecordStatus.Active
        };
    }
}
