using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class CommentConfig : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments", "engagement");

        builder.HasKey(e => e.Id).HasName("pk_comment");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.IdPublic).HasColumnName("id_public").IsRequired().HasColumnType("uuid");
        builder.Property(e => e.Content).HasColumnName("content").HasMaxLength(1000).IsRequired();
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
        builder.Property(e => e.CreatAt).HasColumnName("create_at").IsRequired().HasDefaultValueSql("now()");

        builder.HasOne(e => e.UserNavi)
            .WithMany(u => u.CommentNavi)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_comment_user")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.PostNavi)
            .WithMany(p => p.CommentNavi)
            .HasForeignKey(e => e.PostId)
            .HasConstraintName("fk_comment_post")
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("ck_content_length", "char_length(content) >= 1"));
    }
}