using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class NotiReactPostConfig : IEntityTypeConfiguration<NotiReactPost>
{
    public void Configure(EntityTypeBuilder<NotiReactPost> builder)
    {
        builder.ToTable("noti_reacts", "notifications");

        builder.HasKey(x => x.NotiId).HasName("pk_noti_reacts");
        builder.Property(x => x.NotiId).HasColumnName("noti_id").IsRequired();
        builder.Property(x => x.PostId).HasColumnName("post_id").IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").IsRequired().HasColumnType("enum.react_enum");
        builder.Property(x => x.PreviewContent).HasColumnName("preview_content").HasMaxLength(70);

        builder.HasOne(x => x.NotiNavi)
            .WithOne(x => x.NotiReactNavi)
            .HasForeignKey<NotiReactPost>(x => x.NotiId)
            .HasConstraintName("fk_noti")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PostNavi)
            .WithMany(x => x.NotiReactPostNavi)
            .HasForeignKey(x => x.PostId)
            .HasConstraintName("fk_notireactpost_post")
            .OnDelete(DeleteBehavior.Cascade);
    }
}