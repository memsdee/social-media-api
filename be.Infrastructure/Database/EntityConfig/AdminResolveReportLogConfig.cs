using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class AdminResolveReportLogConfig : IEntityTypeConfiguration<AdminResolveReportLog>
{
    public void Configure(EntityTypeBuilder<AdminResolveReportLog> builder)
    {
        builder.ToTable("admin_resolve_report_log", "logging");

        builder.HasKey(e => e.Id).HasName("pk_admin_resolve_report_log");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.ReportId).HasColumnName("report_id").IsRequired();
        builder.Property(e => e.AdminId).HasColumnName("admin_id").IsRequired();
        builder.Property(e => e.ResolvedAt).HasColumnName("resolved_at").IsRequired().HasDefaultValueSql("now()");

        builder.HasOne(e => e.AccountNavi)
            .WithMany(u => u.AdminResolveReportLogsNavi)
            .HasForeignKey(e => e.AdminId)
            .HasConstraintName("fk_admin_resolve_report_log_user")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ReportNavi)
            .WithMany(r => r.AdminResolveReportLogsNavi)
            .HasForeignKey(e => e.ReportId)
            .HasConstraintName("fk_admin_resolve_report_log_report")
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
            t.HasCheckConstraint("ck_admin_resolve_report_log_fk_positive", "report_id > 0 AND admin_id > 0"));

        builder.HasIndex(e => new { e.ReportId, e.AdminId }).IsUnique().HasDatabaseName("uq_report_admin");
    }
}