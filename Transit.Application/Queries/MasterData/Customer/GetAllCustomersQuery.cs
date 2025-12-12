using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

public class GetAllCustomersQuery : IRequest<OperationResult<List<Customer>>>
{

    public RecordStatus? RecordStatus { get; set; }
}