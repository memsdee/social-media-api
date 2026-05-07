using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class AdminDelAccountLogConfig : IEntityTypeConfiguration<AdminDelAccountLog>
{
    public void Configure(EntityTypeBuilder<AdminDelAccountLog> builder)
    {
        builder.ToTable("admin_del_account_logs", "logging");

        builder.HasKey(e => e.Id).HasName("pk_admin_del_account_logs");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.TargetId).HasColumnName("target_id").IsRequired();
        builder.Property(e => e.AdminId).HasColumnName("admin_id");
        builder.Property(e => e.IsAdmin).HasColumnName("is_admin").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at").IsRequired().HasDefaultValueSql("now()");

        builder.HasOne(e => e.AccountNavi)
            .WithMany(e => e.AdminDelAccountLogsNavi)
            .HasForeignKey(e => e.AdminId)
            .HasConstraintName("fk_accounts_admin_del_account_logs")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.TargetIdNavi)
            .WithMany(e => e.TargetAdminDelAccountLogsNavi)
            .HasForeignKey(e => e.TargetId)
            .HasConstraintName("fk_accounts_target_del_account_navi")
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("ck_admin_del_account_log_fk_positive",
            "target_id > 0 AND (admin_id >= 0 OR admin_id IS NULL)"));

        builder.HasIndex(e => new { e.TargetId, e.AdminId }).IsUnique().HasDatabaseName("uq_target_admin");
    }
}