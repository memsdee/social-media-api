using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class MessageReadConfig : IEntityTypeConfiguration<MessageRead>
{
    public void Configure(EntityTypeBuilder<MessageRead> builder)
    {
        builder.ToTable("message_reads", "chat");
        builder.HasKey(x => new { x.UserId, x.MessageId }).HasName("pk_message_reads");

        builder.Property(x => x.ConversationId).HasColumnName("conversation_id").IsRequired();
        builder.Property(x => x.MessageId).HasColumnName("message_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.ReadAt).HasColumnName("read_at").IsRequired().HasDefaultValueSql("now()");


        builder.HasOne(x => x.MessageNavi)
            .WithMany(m => m.MessageReadsNavi)
            .HasForeignKey(x => x.MessageId)
            .HasConstraintName("fk_readmessage_message")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ConversationNavi)
            .WithMany(c => c.MessageReadNavi)
            .HasForeignKey(x => x.ConversationId)
            .HasConstraintName("fk_readmessage_conversation")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UserNavi)
            .WithMany(u => u.MessageReadsNavi)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_messagereads_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}