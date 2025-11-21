namespace Transit.Domain.Models;
public class Role : BaseEntity
{
    private List<RolePrivilege> _rolePrivileges = new List<RolePrivilege>();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public ICollection<RolePrivilege> RolePrivileges { get { return _rolePrivileges; } }
    public ICollection<UserRole> UserRoles { get; set; }
    public static Role Create(string name, string description)
    {
        var userPrivilege = new Role
        {
            Name = name,
            Description = description,
            RecordStatus = Shared.RecordStatus.Active
        };
        return userPrivilege;
    }
    public void AddRolePrivilege(RolePrivilege roleClaim)
    {
        _rolePrivileges.Add(roleClaim);
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }

}
