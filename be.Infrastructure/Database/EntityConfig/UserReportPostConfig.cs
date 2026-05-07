using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class UserReportPostConfig : IEntityTypeConfiguration<UserReportPost>
{
    public void Configure(EntityTypeBuilder<UserReportPost> builder)
    {
        builder.ToTable("user_report_posts", "engagement");

        builder.HasKey(e => e.Id).HasName("pk_user_reports");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.ReporterId).HasColumnName("reporter_id").IsRequired();
        builder.Property(e => e.ReportedPost).HasColumnName("reported_post").IsRequired();
        builder.Property(e => e.ReportCode).HasColumnName("report_code").IsRequired();
        builder.Property(e => e.OtherReason).HasColumnName("other_reason").HasMaxLength(1000);
        builder.Property(e => e.Status).HasColumnName("status").IsRequired()
            .HasColumnType("enum.status_report_post_enum")
            .HasDefaultValueSql("'pending'::enum.status_report_post_enum");
        builder.Property(e => e.CreateAt).HasColumnName("create_at").IsRequired().HasDefaultValueSql("now()");

        builder.HasOne(e => e.UserNavi)
            .WithMany(u => u.UserReportNavi)
            .HasForeignKey(x => x.ReporterId)
            .HasConstraintName("fk_reports_users")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.PostNavi)
            .WithMany(r => r.UserReportNavi)
            .HasForeignKey(x => x.ReportedPost)
            .HasConstraintName("fk_reports_posts")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.ReasonReportPostNavi)
            .WithMany(r => r.UserReportsNavi)
            .HasForeignKey(x => x.ReportCode)
            .HasConstraintName("fk_reports_report_reasons")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(e => new { e.ReporterId, e.ReportedPost, e.ReportCode }).IsUnique()
            .HasDatabaseName("uq_user_report_once_per_reason");
    }
}