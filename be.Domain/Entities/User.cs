namespace be.Domain.Entities;

public class User
{
    public short Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid? Avatar { get; set; }
    public string? Bio { get; set; }
    public short TotalFollower { get; set; }
    public short TotalFollowing { get; set; }
    public short TotalPost { get; set; }
    public short AccountId { get; set; }

    public virtual Account AccountNavi { get; set; } = null!;
    public virtual ICollection<Feedback> FeedbackNavi { get; set; } = [];
    public virtual ICollection<Post> PostNavi { get; set; } = [];
    public virtual ICollection<Comment> CommentNavi { get; set; } = [];
    public virtual ICollection<ReactPost> ReactNavi { get; set; } = [];
    public virtual ICollection<Follow> FollowersNavi { get; set; } = [];
    public virtual ICollection<Follow> FollowingNavi { get; set; } = [];
    public virtual ICollection<UsernameChangeLog> UsernameChangeLogNavi { get; set; } = [];
    public virtual ICollection<UseridChangeLog> UseridChangeLogNavi { get; set; } = [];
    public virtual ICollection<UserReportPost> UserReportNavi { get; set; } = [];
    public virtual QuestionPlaylist QuestionPlaylistNavi { get; set; } = null!;
    public virtual ICollection<UserQuestionLog> UserQuestionLogsNavi { get; set; } = [];
    public virtual ICollection<Notifications> SenderNotiNavi { get; set; } = [];
    public virtual ICollection<Notifications> ReciverNotiNavi { get; set; } = [];
    public virtual ICollection<ConversationUser> ConversationUsersNavi { get; set; } = [];
    public virtual ICollection<Message> MessagesNavi { get; set; } = [];
    public virtual ICollection<MessageRead> MessageReadsNavi { get; set; } = [];
    public virtual ICollection<AdminDelAccountLog> TargetAdminDelAccountLogsNavi { get; set; } = [];
}