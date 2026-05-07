using be.Domain.Entities;
using be.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class PostConfig : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts", "content");

        builder.HasKey(x => x.Id).HasName("pk_post");
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.IdPublic).HasColumnName("id_public").IsRequired().HasColumnType("uuid");
        builder.Property(x => x.Content).HasColumnName("content").HasMaxLength(2000);
        builder.Property(x => x.TotalComment).HasColumnName("total_comment").IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TotalLike).HasColumnName("total_like").IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TotalDislike).HasColumnName("total_dislike").IsRequired().HasDefaultValue(0);
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.CreatAt).HasColumnName("creat_at").IsRequired().HasDefaultValueSql("now()");
        builder.Property(x => x.ScoreTrend).HasColumnName("score").IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ScoreReport).HasColumnName("score_report").IsRequired().HasDefaultValue(0);
        builder.Property(x => x.Status).HasColumnName("status").IsRequired().HasColumnType("enum.status_post_enum")
            .HasDefaultValueSql("'active'::enum.status_post_enum");

        builder.HasQueryFilter(x =>
            x.Status == StatusPostEnum.active &&
            x.UserNavi.AccountNavi.Status != StatusAccountEnum.deleted);

        builder.HasIndex(x => x.ScoreTrend)
            .HasDatabaseName("idx_post_score_active_only")
            .HasFilter(@"""status"" = 'active'");

        builder.HasOne(x => x.UserNavi)
            .WithMany(y => y.PostNavi)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_post_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}