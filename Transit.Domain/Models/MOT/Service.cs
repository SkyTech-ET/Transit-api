namespace Transit.Domain.Models.MOT;

public class Service : BaseEntity
{
    private readonly List<ServiceStageExecution> _stages = new List<ServiceStageExecution>();
    private readonly List<ServiceDocument> _documents = new List<ServiceDocument>();
    private readonly List<ServiceMessage> _messages = new List<ServiceMessage>();

    public string ServiceNumber { get; private set; } = string.Empty;
    public string ItemDescription { get; private set; } = string.Empty;
    public string RouteCategory { get; private set; } = string.Empty;
    public decimal DeclaredValue { get; private set; }
    public string TaxCategory { get; private set; } = string.Empty;
    public string CountryOfOrigin { get; private set; } = string.Empty;
    public ServiceType ServiceType { get; private set; }
    public ServiceStatus Status { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    
    // Foreign Keys
    public long CustomerId { get; private set; }
    public long? AssignedCaseExecutorId { get; private set; }
    public long? AssignedAssessorId { get; private set; }
    public long CreatedByDataEncoderId { get; private set; }
    
    // Navigation Properties
    public User Customer { get; set; }
    public User? AssignedCaseExecutor { get; set; }
    public User? AssignedAssessor { get; set; }
    public long CreatedByUserId { get; set; }
    
    public ICollection<ServiceStageExecution> Stages => _stages;
    public ICollection<ServiceDocument> Documents => _documents;
    public ICollection<ServiceMessage> Messages => _messages;

    public string AssignmentNotes { get; set; }

    public static Service Create(
        string serviceNumber,
        string itemDescription,
        string routeCategory,
        decimal declaredValue,
        string taxCategory,
        string countryOfOrigin,
        ServiceType serviceType,
        long customerId,
        long createdByUserId)
    {
        return new Service
        {
            ServiceNumber = serviceNumber,
            ItemDescription = itemDescription,
            RouteCategory = routeCategory,
            DeclaredValue = declaredValue,
            TaxCategory = taxCategory,
            CountryOfOrigin = countryOfOrigin,
            ServiceType = serviceType,
            Status = ServiceStatus.Draft,
            RiskLevel = RiskLevel.Blue,
            CustomerId = customerId,
            CreatedByUserId = createdByUserId,
            RecordStatus = RecordStatus.Active
        };
    }

    public void UpdateStatus(ServiceStatus status)
    {
        Status = status;
        UpdateAudit("System");
    }

    public void AssignCaseExecutor(long caseExecutorId)
    {
        AssignedCaseExecutorId = caseExecutorId;
        UpdateAudit("System");
    }

    public void AssignAssessor(long assessorId)
    {
        AssignedAssessorId = assessorId;
        UpdateAudit("System");
    }

    public void UpdateRiskLevel(RiskLevel riskLevel)
    {
        RiskLevel = riskLevel;
        UpdateAudit("System");
    }

    public void AddStage(ServiceStageExecution stage)
    {
        _stages.Add(stage);
    }

    public void AddDocument(ServiceDocument document)
    {
        _documents.Add(document);
    }

    public void AddMessage(ServiceMessage message)
    {
        _messages.Add(message);
    }

    public void SetRiskLevel(RiskLevel riskLevel)
    {
        RiskLevel = riskLevel;
        UpdateAudit("System");
    }

    public void UpdateDetails(
        string itemDescription,
        string routeCategory,
        decimal declaredValue,
        string taxCategory,
        string countryOfOrigin)
    {
        ItemDescription = itemDescription;
        RouteCategory = routeCategory;
        DeclaredValue = declaredValue;
        TaxCategory = taxCategory;
        CountryOfOrigin = countryOfOrigin;
        UpdateAudit("System");
    }
}
