namespace Transit.Domain.Models.Shared;

public enum RecordStatus
{
    InActive = 1,
    Active = 2,
    Deleted
}
public enum AccountStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
}
public enum OrderStatus
{
    Pending = 1, AcceptedByAgent = 2, AvailableToCRO = 3, AcceptedByCRO = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Cancelled = 4,
    UnPaid = 5
}

public enum PaymentOption
{
    Online = 1,
    Cash = 2

}

public enum PlatformType
{
    Invoice,
    Table,
    Other
}
public enum Usage
{
    House = 1,
    Commercial = 2
}
public enum PropertyType
{
    Rent = 1, Sell, Lease
}
public enum PropertyStatus
{
    Available = 1,
    RentedOrSold = 2,
    LeasedOutside = 3
}
public enum RoleName
{
    SuperAdmin = 1,
    StoreAdmin = 2,
    Agent = 3,
    CRO = 4,
    Operator = 5,
    Customer = 6,
    // MOT System Roles
    Manager = 7,
    CaseExecutor = 8,
    Assessor = 9,
    DataEncoder = 10,
    Importer = 11
}

// MOT Service Management Enums
public enum ServiceType
{
    Multimodal = 1,
    Unimodal = 2
}

public enum ServiceStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    InProgress = 5,
    Completed = 6,
    Rejected = 7,
    Cancelled = 8
}

public enum ServiceStage
{
    PrepaymentInvoice = 1,
    DropRisk = 2,
    DeliveryOrder = 3,
    Inspection = 4,
    Emergency = 5,
    Exit = 6,
    Transportation = 7,
    Clearance = 8,
    LocalPermission = 9, // Unimodal only
    Arrival = 10, // Unimodal only
    StoreSettlement = 11
}

public enum StageStatus
{
    NotStarted = 1,
    Pending = 2,
    InProgress = 3,
    Completed = 4,
    Blocked = 5,
    NeedsReview = 6
}

public enum RiskLevel
{
    Blue = 1,
    Green = 2,
    Yellow = 3,
    Red = 4
}

public enum NotificationType
{
    ServiceUpdate = 1,
    DocumentUpload = 2,
    StatusChange = 3,
    PaymentReminder = 4,
    SystemAlert = 5,
    Message = 6
}

public enum MessageType
{
    Group = 1,
    Direct = 2,
    System = 3
}

public enum DocumentType
{
    Invoice = 1,
    BankReceipt = 2,
    DeliveryOrder = 3,
    InspectionReport = 4,
    ClearanceDocument = 5,
    TransportLicense = 6,
    ArrivalPhoto = 7,
    StoreReceipt = 8,
    LegalDocument = 9,
    Other = 10
}