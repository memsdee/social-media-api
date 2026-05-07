using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class ReactPostConfig : IEntityTypeConfiguration<ReactPost>
{
    public void Configure(EntityTypeBuilder<ReactPost> builder)
    {
        builder.ToTable("react_posts", "engagement");

        builder.HasKey(t => t.Id).HasName("pk_react");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.Type).HasColumnName("type").IsRequired().HasColumnType("enum.react_enum");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
        builder.Property(e => e.CreatAt).HasColumnName("creat_at").IsRequired().HasDefaultValueSql("now()");

        builder.HasOne(x => x.UserNavi)
            .WithMany(u => u.ReactNavi)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_react_user")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PostNavi)
            .WithMany(y => y.ReacPostNavi)
            .HasForeignKey(x => x.PostId)
            .HasConstraintName("fk_react_post")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.PostId }).IsUnique().HasDatabaseName("uq_react_user_post");
    }
}