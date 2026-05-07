using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class FollowConfig : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("follows", "engagement");

        builder.HasKey(f => f.Id).HasName("pk_follow");

        builder.Property(f => f.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(f => f.FollowerId)
            .HasColumnName("follower_id")
            .IsRequired();
        builder.Property(f => f.FolloweeId)
            .HasColumnName("followee_id")
            .IsRequired();
        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.HasIndex(f => new { f.FollowerId, f.FolloweeId })
            .HasDatabaseName("uq_follows_follower_followee")
            .IsUnique();

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_follow_no_self",
            "\"follower_id\" <> \"followee_id\""
        ));

        builder.HasOne(x => x.FollowerNavi)
            .WithMany(u => u.FollowingNavi)
            .HasForeignKey(x => x.FollowerId)
            .HasConstraintName("fk_follwer_user")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FolloweeNavi)
            .WithMany(u => u.FollowersNavi)
            .HasForeignKey(x => x.FolloweeId)
            .HasConstraintName("fk_following_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}