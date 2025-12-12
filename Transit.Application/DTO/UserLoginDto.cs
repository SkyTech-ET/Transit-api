using Transit.Domain.Models.Shared;

namespace Transit.Application.Dtos
{
    public class UserLoginDto
    {
        public UserLoginDto()
        {

        }
        public UserLoginDto(string userName, string email, string firstName, string lastName, string phone, string profilePhoto)
        {
            Username = userName;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            ProfilePhoto = profilePhoto;
            Roles = new List<RoleDto>();

        }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ProfilePhoto { get; set; } = string.Empty;
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long Id { get; set; }
        public OrganizationDetailDto Organization { get; set; }
        public List<RoleDto> Roles { get; set; }

    }

    public class RoleDto
    {
        public RoleDto()
        {
            Privileges = new List<PrivilegeDto>();
        }
        public long Id { get; set; }
        public string RoleName { get; set; }
        public List<PrivilegeDto> Privileges { get; set; }
    }


    public class PrivilegeDto
    {
        public long Id { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
    }

    public class OrganizationDetailDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string LogoPath { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public SubscriptionDetail Subscription { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }

    public class SubscriptionDetail
    {
        public long Id { get; set; }
        public string PlanName { get; set; }
        public double Price { get; set; }
        public DateTime EffectiveDateFrom { get; set; }
        public DateTime EffectiveDateTo { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public bool Categorize { get; set; }
        public bool Formula { get; set; }
        public int PageLimit { get; set; }
    }

}
