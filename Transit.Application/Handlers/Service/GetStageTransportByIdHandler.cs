using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

    internal class GetStageTransportByIdHandler : IRequestHandler<GetStageTransportByIdQuery, OperationResult<StageTransport>>
    {
        private readonly ApplicationDbContext _context;

        public GetStageTransportByIdHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<StageTransport>> Handle(GetStageTransportByIdQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<StageTransport>();

            var stageTransports = await _context.StageTransports
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (stageTransports == null)
            {
                result.AddError(ErrorCode.NotFound, "Stage Transport not found.");
                return result;
            }

            result.Payload = stageTransports;
            result.Message = "Operation success";
            return result;
        }
    }

