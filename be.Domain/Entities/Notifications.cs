using be.Domain.Enums;

namespace be.Domain.Entities;

public class Notifications
{
    public short Id { get; set; }
    public short SenderId { get; set; }
    public short ReciverId { get; set; }
    public Guid? ThumbnailNoti { get; set; }
    public NotiTargetEnum Target { get; set; }
    public NotiActionEnum Action { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    public virtual User SenderNavi { get; set; } = null!;
    public virtual User ReciverNavi { get; set; } = null!;
    public virtual NotiCmt NotiCmtNavi { get; set; } = null!;
    public virtual NotiReactPost NotiReactNavi { get; set; } = null!;
}