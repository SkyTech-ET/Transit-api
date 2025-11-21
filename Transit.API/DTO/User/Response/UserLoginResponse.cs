namespace Transit.Api.Contracts.User.Response
{
    public class UserLoginResponse
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string userToken { get; set; }
        public string RefreshToken { get; set; }
        public List<RoleResponse> Roles { get; set; }

    }

    public class RoleResponse
    {
        public RoleResponse()
        {
            Claims = new List<userPrivilegeResponse>();
        }
        public string RoleName { get; set; }
        public List<userPrivilegeResponse> Claims { get; set; }
    }

    public class userPrivilegeResponse
    {
        public long Id { get; set; }
        public string Claim { get; set; }
    }
}

