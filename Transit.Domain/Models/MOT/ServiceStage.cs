namespace Transit.Domain.Models.MOT;

public class ServiceStageExecution : BaseEntity
{
    private readonly List<StageDocument> _documents = new List<StageDocument>();
    private readonly List<StageComment> _comments = new List<StageComment>();

    public Shared.ServiceStage Stage { get; private set; }
    public StageStatus Status { get; private set; }
    public inspectionType? InspectionType { get; private set; }
    public StageSpot? StageSpot { get; private set; }
    public string? Comments { get; private set; }
    public string? SpotComment { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? AssignedTo { get; private set; }
    public string? RiskNotes { get; private set; }
    public bool RequiresCustomerAction { get; private set; }
    public bool IsBlocked { get; private set; }
    public string? BlockReason { get; private set; }

    // Foreign Keys
    public long? ServiceId { get; private set; }
    public long? UpdatedByUserId { get; private set; }

    // Navigation Properties
    public Service? Service { get; set; }
    public User? UpdatedByUser { get; set; }
    
    public ICollection<StageDocument> Documents => _documents;
    public ICollection<StageComment> StageComments => _comments;

    public DateTime CreatedDate { get; set; }

    public static ServiceStageExecution Create(
        long serviceId,
        Shared.ServiceStage stage,
        long? updatedByUserId = null)
    {
        return new ServiceStageExecution
        {
            ServiceId = serviceId,
            Stage = stage,
            Status = StageStatus.NotStarted,
            UpdatedByUserId = updatedByUserId,
            RecordStatus = RecordStatus.Active
        };
    }

    public void UpdateStatus(StageStatus status, string? comments = null)
    {
        Status = status;
        Comments = comments;
        
        if (status == StageStatus.InProgress && !StartedAt.HasValue)
        {
            StartedAt = DateTime.UtcNow;
        }
        
        if (status == StageStatus.Completed && !CompletedAt.HasValue)
        {
            CompletedAt = DateTime.UtcNow;
        }
        
        UpdateAudit("System");
    }

    public void SetBlocked(bool isBlocked, string? reason = null)
    {
        IsBlocked = isBlocked;
        BlockReason = reason;
        UpdateAudit("System");
    }

    public void SetCustomerActionRequired(bool required)
    {
        RequiresCustomerAction = required;
        UpdateAudit("System");
    }

    public void AddRiskNotes(string notes)
    {
        RiskNotes = notes;
        UpdateAudit("System");
    }

    public void AddDocument(StageDocument document)
    {
        _documents.Add(document);
    }

    public void AddComment(StageComment comment)
    {
        _comments.Add(comment);
    }
}
