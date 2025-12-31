namespace Transit.Domain.Models.MOT;

public class Notification : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public bool IsSent { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? ActionText { get; private set; }
    public bool IsUrgent { get; private set; }

    // Foreign Keys
    public long? UserId { get; private set; }
    public long? ServiceId { get; private set; }
    public long? ServiceStageId { get; private set; }

    // Navigation Properties
    public User? User { get; set; }
    public Service? Service { get; set; }
    public ServiceStageExecution? ServiceStage { get; set; }

    public static Notification Create(
        string title,
        string message,
        NotificationType type,
        long userId,
        long? serviceId = null,
        long? serviceStageId = null,
        string? actionUrl = null,
        string? actionText = null,
        bool isUrgent = false)
    {
        return new Notification
        {
            Title = title,
            Message = message,
            Type = type,
            UserId = userId,
            ServiceId = serviceId,
            ServiceStageId = serviceStageId,
            ActionUrl = actionUrl,
            ActionText = actionText,
            IsUrgent = isUrgent,
            IsRead = false,
            IsSent = false,
            RecordStatus = RecordStatus.Active
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        UpdateAudit("System");
    }

    public void MarkAsSent()
    {
        IsSent = true;
        SentAt = DateTime.UtcNow;
        UpdateAudit("System");
    }

    public void UpdateMessage(string message)
    {
        Message = message;
        UpdateAudit("System");
    }
}
