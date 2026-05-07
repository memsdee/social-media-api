using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class AdminDelPostLogConfig : IEntityTypeConfiguration<AdminDelPostLog>
{
    public void Configure(EntityTypeBuilder<AdminDelPostLog> builder)
    {
        builder.ToTable("admin_del_post_logs", "logging");

        builder.HasKey(e => e.Id).HasName("pk_admin_del_post_logs");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
        builder.Property(e => e.AdminId).HasColumnName("admin_id");
        builder.Property(e => e.IsAdmin).HasColumnName("is_admin").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at").IsRequired().HasDefaultValueSql("now()");

        builder.HasOne(e => e.AccountNavi)
            .WithMany(e => e.AdminDelPostLogsNavi)
            .HasForeignKey(e => e.AdminId)
            .HasConstraintName("fk_accounts_admin_del_post_logs")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PostNavi)
            .WithMany(e => e.AdminDelPostLogNavi)
            .HasForeignKey(e => e.PostId)
            .HasConstraintName("fk_accounts_post_del_post_navi")
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("ck_admin_del_post_log_fk_positive",
            "post_id > 0 AND (admin_id >= 0 OR admin_id IS NULL)"));

        builder.HasIndex(e => new { e.PostId, e.AdminId }).IsUnique().HasDatabaseName("uq_post_admin");
    }
}