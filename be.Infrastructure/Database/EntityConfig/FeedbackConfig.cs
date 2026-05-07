using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class FeedbackConfig : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("feedbacks", "support");

        builder.HasKey(e => e.Id).HasName("pk_feedback");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.Content).HasColumnName("content").HasColumnType("text").IsRequired().HasMaxLength(5000);
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(e => e.UserNavi)
            .WithMany(u => u.FeedbackNavi)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_feedback_user")
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("ck_content_length", "char_length(content) >= 20"));
    }
}