namespace Transit.Domain.Models;

public class RolePrivilege : BaseEntity
{
    public long RoleId { get; set; }
    public long PrivilegeId { get; set; }
    public virtual Privilege Privilege { get; set; }
    public virtual Role Role { get; set; }

    public static RolePrivilege Create(long roleId, long privilegeId)
    {
        return new RolePrivilege
        {
            RoleId = roleId,
            PrivilegeId = privilegeId,
            RecordStatus = Shared.RecordStatus.Active
        };
    }
}
