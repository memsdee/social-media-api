using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class UseridChangeLogConfig : IEntityTypeConfiguration<UseridChangeLog>
{
    public void Configure(EntityTypeBuilder<UseridChangeLog> builder)
    {
        builder.ToTable("userid_change_log", "logging");

        builder.HasKey(e => e.Id).HasName("pk_userid_change_log");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.OldUserId).HasColumnName("old_userid").IsRequired().HasMaxLength(50);
        builder.Property(e => e.NewUserId).HasColumnName("new_userid").IsRequired().HasMaxLength(50);
        builder.Property(e => e.ChangedAt).HasColumnName("changed_at").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();

        builder.HasOne(e => e.UserNavi)
            .WithMany(o => o.UseridChangeLogNavi)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_userid_change_log_user")
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(p => p.HasCheckConstraint("ck_old_userid_type",
            "char_length(old_userid) BETWEEN 3 AND 50 " + "AND old_userid ~ '^[A-Za-z0-9_]+$'"));
        builder.ToTable(p => p.HasCheckConstraint("ck_new_userid_type",
            "char_length(new_userid) BETWEEN 3 AND 50 " + "AND new_userid ~ '^[A-Za-z0-9_]+$'"));
    }
}