using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class QuestionConfig : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions", "questions");

        builder.HasKey(q => q.Id).HasName("pk_questions");

        builder.Property(q => q.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(q => q.Content).HasColumnName("content").IsRequired().HasMaxLength(500);

        builder.HasIndex(u => u.Content, "uq_content").IsUnique();
    }
}