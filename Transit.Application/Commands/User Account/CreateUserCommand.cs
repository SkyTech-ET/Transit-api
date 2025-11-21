namespace Transit.Application;
public class CreateUserCommand : IRequest<OperationResult<User>>
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ProfilePhoto { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
    public bool IsSuperAdmin { get; set; } = false;
    public long? StoreId { get; set; }
    public List<long> Roles { get; set; }

}
