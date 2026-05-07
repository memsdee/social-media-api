using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class ConversationUserConfig : IEntityTypeConfiguration<ConversationUser>
{
    public void Configure(EntityTypeBuilder<ConversationUser> builder)
    {
        builder.ToTable("conversation_participants", "chat");

        builder.HasKey(e => e.Id).HasName("pk_conversation_users");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.ConversationId).HasColumnName("conversation_id").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.UnreadCount).HasColumnName("unread_count").IsRequired().HasDefaultValue(0);
        builder.Property(e => e.JoinedAt).HasColumnName("joined_at").IsRequired().HasDefaultValueSql("now()");

        builder.HasIndex(e => new { e.UserId, e.ConversationId })
            .IsUnique().HasDatabaseName("ux_conversation_participants_conversation_id_user_id");

        builder.HasOne(e => e.UsersNavi)
            .WithMany(u => u.ConversationUsersNavi)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_conversation_participants_user_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ConversationsNavi)
            .WithMany(x => x.ConversationUserNavi)
            .HasForeignKey(e => e.ConversationId)
            .HasConstraintName("fk_conversation_participants_conversation_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}