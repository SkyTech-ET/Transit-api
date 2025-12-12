using Transit.Domain.Models.Shared;

namespace Transit.Api.Contracts.User.Response
{
    public class RoleDetail
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RecordStatus RecordStatus { get; set; }
        //public string? RecordStatusDescription => RecordStatus.GetDisplayName();
        public List<PrivilegeDetail> Privileges { get; set; }
    }

}
