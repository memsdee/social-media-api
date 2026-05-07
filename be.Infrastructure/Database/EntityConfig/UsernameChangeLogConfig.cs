using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class UsernameChangeLogConfig : IEntityTypeConfiguration<UsernameChangeLog>
{
    public void Configure(EntityTypeBuilder<UsernameChangeLog> builder)
    {
        builder.ToTable("username_change_log", "logging");

        builder.HasKey(e => e.Id).HasName("pk_user_username_change_log");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.OldUsername).HasColumnName("old_username").IsRequired().HasMaxLength(50);
        builder.Property(e => e.NewUsername).HasColumnName("new_username").IsRequired().HasMaxLength(50);
        builder.Property(e => e.ChangeAt).HasColumnName("change_at").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();

        builder.HasOne(e => e.UserNavi)
            .WithMany(o => o.UsernameChangeLogNavi)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_username_change_log_user")
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(p => p.HasCheckConstraint("ck_old_username_type",
            "char_length(old_username) BETWEEN 3 AND 50 " + "AND old_username !~ '^\\s' " +
            "AND old_username !~ '\\s$' " + "AND old_username !~ '\\s{2,}'"));
        builder.ToTable(p => p.HasCheckConstraint("ck_new_username_type",
            "char_length(new_username) BETWEEN 3 AND 50 " + "AND new_username !~ '^\\s' " +
            "AND new_username !~ '\\s$' " + "AND new_username !~ '\\s{2,}'"));
    }
}