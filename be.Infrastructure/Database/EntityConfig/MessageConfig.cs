using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class MessageConfig : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages", "chat");

        builder.HasKey(x => x.Id).HasName("pk_messages");

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.ConversationId).HasColumnName("conversation_id").IsRequired();
        builder.Property(x => x.SenderId).HasColumnName("sender_id").IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").IsRequired().HasColumnType("text").HasMaxLength(5000);
        builder.Property(x => x.Type).HasColumnName("type").IsRequired().HasColumnType("enum.type_message_enum");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("now()");

        builder.ToTable(t => t.HasCheckConstraint("ck_message_content_length", "char_length(content) <= 5000"));

        builder.HasOne(x => x.ConversationNavi)
            .WithMany(c => c.MessagesNavi)
            .HasForeignKey(x => x.ConversationId)
            .HasConstraintName("fk_messages_conversation_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SenderNavi)
            .WithMany(u => u.MessagesNavi)
            .HasForeignKey(x => x.SenderId)
            .HasConstraintName("fk_messages_sender_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}