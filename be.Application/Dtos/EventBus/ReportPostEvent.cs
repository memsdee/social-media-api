using be.Domain.Enums;

namespace be.Application.Dtos.EventBus;

public class ReportPostEvent
{
    public List<ReportPostItemEvent> Reports { get; set; } = [];
}

public class ReportPostItemEvent
{
    public short Id { get; set; }
    public string ReporterPublicId { get; set; } = null!;
    public string ReporterName { get; set; } = null!;
    public Guid? ReporterAvatar { get; set; }
    public string ReporterMail { get; set; } = null!;
    public Guid PostPublicId { get; set; }
    public short PostSequenceId { get; set; }
    public short ReasonCode { get; set; }
    public string? OtherReason { get; set; }
    public StatusReportPostEnum Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}