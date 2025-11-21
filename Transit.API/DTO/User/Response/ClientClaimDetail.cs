using Transit.Domain.Models.Shared;

namespace Transit.Api.Contracts.User.Response
{
    public class userPrivilegeDetail
    {
        public long Id { get; set; }
        public string Name { get; private set; }
        public string Claim { get; private set; }
        public string Description { get; private set; }
        public long ClientId { get; private set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
