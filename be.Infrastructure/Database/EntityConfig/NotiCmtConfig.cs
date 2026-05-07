using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class NotiCmtConfig : IEntityTypeConfiguration<NotiCmt>
{
    public void Configure(EntityTypeBuilder<NotiCmt> builder)
    {
        builder.ToTable("noti_cmts", "notifications");

        builder.HasKey(x => x.NotiId).HasName("pk_cmts");
        builder.Property(x => x.NotiId).HasColumnName("noti_id");
        builder.Property(x => x.PostId).HasColumnName("post_id").IsRequired();
        builder.Property(x => x.CmtId).HasColumnName("cmt_id").IsRequired();
        builder.Property(x => x.Preview).HasColumnName("preview").IsRequired().HasMaxLength(70);

        builder.HasOne(x => x.NotiNavi)
            .WithOne(x => x.NotiCmtNavi)
            .HasForeignKey<NotiCmt>(x => x.NotiId)
            .HasConstraintName("fk_noti")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PostNavi)
            .WithMany(x => x.NotiCmtNavi)
            .HasForeignKey(x => x.PostId)
            .HasConstraintName("fk_noticmt_post")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CmtNavi)
            .WithMany(x => x.NotiCmtNavi)
            .HasForeignKey(x => x.CmtId)
            .HasConstraintName("fk_noticmt_cmt")
            .OnDelete(DeleteBehavior.Cascade);
    }
}