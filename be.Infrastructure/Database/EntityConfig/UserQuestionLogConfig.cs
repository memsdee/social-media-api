using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class UserQuestionLogConfig : IEntityTypeConfiguration<UserQuestionLog>
{
    public void Configure(EntityTypeBuilder<UserQuestionLog> builder)
    {
        builder.ToTable("user_question_logs", "logging");

        builder.HasKey(uql => uql.Id).HasName("pk_user_question_logs");
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.QuestionId).HasColumnName("question_id").IsRequired();
        builder.Property(x => x.TotalQuestions).HasColumnName("total_questions").IsRequired();
        builder.Property(x => x.ShowAt).HasColumnName("show_at").IsRequired().HasDefaultValueSql("now()");

        builder.HasOne(uql => uql.UserNavi)
            .WithMany(u => u.UserQuestionLogsNavi)
            .HasForeignKey(uql => uql.UserId)
            .HasConstraintName("fk_user_question_logs_users")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uql => uql.QuestionNavi)
            .WithMany(q => q.UserQuestionLogNavi)
            .HasForeignKey(uql => uql.QuestionId)
            .HasConstraintName("fk_user_question_logs_questions")
            .OnDelete(DeleteBehavior.Cascade);
    }
}