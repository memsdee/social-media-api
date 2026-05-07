using be.Domain.Enums;

namespace be.Domain.Entities;

public class Post
{
    public short Id { get; set; }
    public Guid IdPublic { get; set; }
    public string? Content { get; set; }
    public short TotalComment { get; set; }
    public short TotalLike { get; set; }
    public short TotalDislike { get; set; }
    public short UserId { get; set; }
    public DateTimeOffset CreatAt { get; set; }
    public short ScoreTrend { get; set; }
    public short ScoreReport { get; set; }
    public StatusPostEnum Status { get; set; }

    public virtual User UserNavi { get; set; } = null!;
    public virtual ICollection<Comment> CommentNavi { get; set; } = [];
    public virtual ICollection<ReactPost> ReacPostNavi { get; set; } = [];
    public virtual ICollection<PostImage> PostImageNavi { get; set; } = [];
    public virtual ICollection<UserReportPost> UserReportNavi { get; set; } = [];
    public virtual ICollection<NotiCmt> NotiCmtNavi { get; set; } = [];
    public virtual ICollection<NotiReactPost> NotiReactPostNavi { get; set; } = [];
    public virtual ICollection<AdminDelPostLog> AdminDelPostLogNavi { get; set; } = [];
}