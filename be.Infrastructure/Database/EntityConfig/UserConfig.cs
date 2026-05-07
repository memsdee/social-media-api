using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "user");

        builder.HasKey(u => u.Id).HasName("pk_users");

        builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(u => u.UserId).HasColumnName("user_id").HasMaxLength(50).IsRequired();
        builder.Property(u => u.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(u => u.Avatar).HasColumnName("avatar");
        builder.Property(u => u.Bio).HasColumnName("bio").HasMaxLength(160);
        builder.Property(u => u.TotalFollower).HasColumnName("total_follower").IsRequired().HasDefaultValue(0);
        builder.Property(u => u.TotalFollowing).HasColumnName("total_following").IsRequired().HasDefaultValue(0);
        builder.Property(u => u.TotalPost).HasColumnName("total_post").IsRequired().HasDefaultValue(0);
        builder.Property(u => u.AccountId).HasColumnName("account_id").IsRequired().HasColumnType("smallint");

        builder.HasOne(p => p.AccountNavi)
            .WithOne(p => p.UserNavi)
            .HasForeignKey<User>(p => p.AccountId)
            .HasConstraintName("fk_users_accounts")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId, "uq_userid").IsUnique();

        builder.ToTable(p => p.HasCheckConstraint(
            "ck_user_name_type",
            "char_length(name) BETWEEN 3 AND 50 " +
            "AND name !~ '^\\s' " +
            "AND name !~ '\\s$' " +
            "AND name !~ '\\s{2,}'"));

        builder.ToTable(p => p.HasCheckConstraint(
            "ck_user_id_type",
            "char_length(user_id) BETWEEN 3 AND 50 " +
            "AND user_id ~ '^[A-Za-z0-9_]+$'"));

        builder.ToTable(p => p.HasCheckConstraint(
            "ck_users_total_non_negative",
            "total_follower >= 0 AND total_following >= 0 AND total_post >= 0"));
    }
}