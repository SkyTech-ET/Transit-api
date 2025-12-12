using Transit.Domain.Models.Shared;

namespace Transit.Api;
public class PrivilegeDetail
{
    public long Id { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
