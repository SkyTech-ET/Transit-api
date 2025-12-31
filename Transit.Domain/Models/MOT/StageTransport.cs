using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;

namespace Transit.Domain;

public class StageTransport : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string LicenceDocument { get; private set; } = string.Empty;
    public string PlateNumber { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public ProductAmount ProductAmount { get; private set; }
    public long? ServiceStageId { get; private set; }
    public ServiceStage? ServiceStage { get; private set; }
    public static StageTransport Create(
        string fullName,
        string licenceDocument,
        string plateNumber,
        string phoneNumber,
        ProductAmount productAmount,
        long? serviceSatgeId)
    {
        return new StageTransport
        {
            FullName = fullName,
            LicenceDocument = licenceDocument,
            PlateNumber = plateNumber,
            PhoneNumber = phoneNumber,
            ProductAmount = productAmount,
            ServiceStageId= serviceSatgeId,
            RecordStatus = RecordStatus.Active
        };
    }
    public void UpdateTransport(
        string fullName,
        string licenceDocument,
        string plateNumber,
        string phoneNumber,
        ProductAmount productAmount,
        long? serviceSatgeId,
        RecordStatus recordStatus
    )
    {
        FullName = fullName;
        LicenceDocument = licenceDocument;
        PlateNumber = plateNumber;
        PhoneNumber = phoneNumber;
        ProductAmount = productAmount;
        ServiceStageId = serviceSatgeId;
        RecordStatus = recordStatus;
    }

}
