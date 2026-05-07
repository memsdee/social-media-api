using be.Domain.Entities;
using be.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class PostImageConfig : IEntityTypeConfiguration<PostImage>
{
    public void Configure(EntityTypeBuilder<PostImage> builder)
    {
        builder.ToTable("post_images", "content");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        builder.Property(i => i.Image)
            .HasColumnName("image")
            .HasColumnType("uuid")
            .IsRequired();
        builder.Property(i => i.Type)
            .HasColumnName("type")
            .HasColumnType("enum.image_enum")
            .IsRequired();
        builder.Property(i => i.GroupId)
            .HasColumnName("group_id");
        builder.Property(i => i.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.HasOne(i => i.PostNavi)
            .WithMany(p => p.PostImageNavi)
            .HasForeignKey(i => i.PostId)
            .HasConstraintName("fk_post_image_post")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.PostNavi.UserNavi.AccountNavi.Status != StatusAccountEnum.deleted);
    }
}