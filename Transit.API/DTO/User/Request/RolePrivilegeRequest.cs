using System.ComponentModel.DataAnnotations;
namespace Transit.Api
{
    public class RolePrivilegeRequest
    {
        [Required]
        public long RoleId { get; set; }

        [Required]
        public List<long> Privileges { get; set; }
        public RolePrivilegeRequest()
        {
            Privileges = new List<long>();
        }
    }


}
