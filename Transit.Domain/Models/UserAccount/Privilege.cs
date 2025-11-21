namespace Transit.Domain.Models;

public class Privilege : BaseEntity
{
    public string Action { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    //factory
    public static Privilege Create(string name, string descripton)
    {
        var userPrivilege = new Privilege
        {
            Action = name,
            Description = descripton,
        };
        return userPrivilege;
    }

    public void Update(string name, string description)
    {
        Action = name;
        Description = description;
    }
}
