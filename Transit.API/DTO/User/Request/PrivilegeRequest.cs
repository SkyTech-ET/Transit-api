using Transit.Domain.Models.Shared;
using System.ComponentModel.DataAnnotations;
namespace Transit.Api;

public class PrivilegeRequest
{
    public long Id { get; set; }
    [Required]
    public string Action { get; set; }
    [Required]
    public string Description { get; set; }

    public RecordStatus? RecordStatus { get; set; }

}
