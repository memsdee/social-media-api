using be.Domain.Entities;
using be.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Database;

public class WriteContext : DbContext
{
    public WriteContext()
    {
    }

    public WriteContext(DbContextOptions<WriteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Token> Tokens { get; set; }
    public virtual DbSet<ThirdPartyLogin> ThirdPartyLogins { get; set; }
    public virtual DbSet<Feedback> Feedbacks { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<Post> Posts { get; set; }
    public virtual DbSet<ReactPost> ReactPosts { get; set; }
    public virtual DbSet<Follow> Follows { get; set; }
    public virtual DbSet<PostImage> PostImages { get; set; }
    public virtual DbSet<Question> Questions { get; set; }
    public virtual DbSet<ReasonReportPost> ReasonReportPost { get; set; }
    public virtual DbSet<UserReportPost> UserReportPost { get; set; }
    public virtual DbSet<QuestionPlaylist> QuestionPlaylists { get; set; }
    public virtual DbSet<UserQuestionLog> UserQuestionLogs { get; set; }
    public virtual DbSet<UsernameChangeLog> UsernameChangeLogs { get; set; }
    public virtual DbSet<UseridChangeLog> UseridChangeLogs { get; set; }
    public virtual DbSet<Notifications> Notifications { get; set; }
    public virtual DbSet<NotiCmt> NotiCmts { get; set; }
    public virtual DbSet<NotiReactPost> NotiReactPosts { get; set; }
    public virtual DbSet<Conversations> Conversations { get; set; }
    public virtual DbSet<ConversationUser> ConversationUsers { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<MessageRead> MessageReads { get; set; }
    public virtual DbSet<AdminResolveReportLog> AdminResolveReportLogs { get; set; }
    public virtual DbSet<AdminDelPostLog> AdminDeletePostLogs { get; set; }
    public virtual DbSet<AdminDelAccountLog> AdminDelAccountLogs { get; set; }
    public virtual DbSet<Outbox> Outbox { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<RoleEnum>("enum", "role_enum");
        modelBuilder.HasPostgresEnum<ReactEnum>("enum", "react_enum");
        modelBuilder.HasPostgresEnum<ImageEnum>("enum", "image_enum");
        modelBuilder.HasPostgresEnum<NotiTargetEnum>("enum", "noti_target_enum");
        modelBuilder.HasPostgresEnum<NotiActionEnum>("enum", "noti_action_enum");
        modelBuilder.HasPostgresEnum<TypeMessageEnum>("enum", "type_message_enum");
        modelBuilder.HasPostgresEnum<TypeConversationEnum>("enum", "type_conversation_enum");
        modelBuilder.HasPostgresEnum<StatusReportPostEnum>("enum", "status_report_post_enum");
        modelBuilder.HasPostgresEnum<StatusAccountEnum>("enum", "status_account_enum");
        modelBuilder.HasPostgresEnum<StatusPostEnum>("enum", "status_post_enum");
        modelBuilder.HasPostgresEnum<ThirdPartyLoginEnum>("enum", "third_party_login_enum");
        modelBuilder.HasPostgresEnum<OutboxTopicEnum>("enum", "outbox_topic_enum");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WriteContext).Assembly);
    }
}