namespace Transit.Domain.Models;

public class BaseEntity
{
    public long Id { get; set; }

    //Auditlog
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; } = DateTime.MaxValue;
    public string TimeZoneInfo { get; set; } = string.Empty;
    public DateTime RegisteredDate { get; set; }
    public string RegisteredBy { get; set; } = string.Empty;
    public DateTime LastUpdateDate { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public RecordStatus RecordStatus { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public bool IsReadOnly { get; set; }

    public BaseEntity()
    {
        StartDate = DateTime.UtcNow;
        EndDate = DateTime.MaxValue;
        RegisteredDate = DateTime.UtcNow;
        LastUpdateDate = DateTime.UtcNow;
        IsReadOnly = false;
        RecordStatus = RecordStatus.Active;
        AccountStatus = AccountStatus.Pending;

    }
    public virtual void AddAudit(string updateBy)
    {
        RegisteredDate = DateTime.UtcNow;
        RegisteredBy = updateBy;
    }
    public virtual void UpdateAudit(string updateBy)
    {
        LastUpdateDate = DateTime.UtcNow;
        UpdatedBy = updateBy;
    }
    public void UpdateStatus(RecordStatus recordStatus)
    {
        RecordStatus = recordStatus;
    }
}
