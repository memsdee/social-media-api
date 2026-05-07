using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class ConversationConfig : IEntityTypeConfiguration<Conversations>
{
    public void Configure(EntityTypeBuilder<Conversations> builder)
    {
        builder.ToTable("conversations", "chat");

        builder.HasKey(e => e.Id).HasName("pk_conversation");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.IdPublic).HasColumnName("id_public").IsRequired();
        builder.Property(e => e.LastMessage).HasColumnName("last_message").HasColumnType("text").HasMaxLength(70);
        builder.Property(e => e.CreatorId).HasColumnName("creator_id").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(e => e.Type).HasColumnName("type").IsRequired().HasColumnType("enum.type_conversation_enum");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.KeyParticipants).HasColumnName("key_participant").HasColumnType("bigint");

        builder.HasIndex(e => e.KeyParticipants)
            .IsUnique()
            .HasDatabaseName("ix_conversations_key_participants");

        builder.ToTable(t => t
            .HasCheckConstraint(
                "ck_single_conversation_key_participants",
                "type != 'single' OR (type = 'single' AND key_participant IS NOT NULL)"
            ));

        builder.ToTable(t => t
            .HasCheckConstraint("ck_last_message_length", "char_length(last_message) <= 70"));
    }
}