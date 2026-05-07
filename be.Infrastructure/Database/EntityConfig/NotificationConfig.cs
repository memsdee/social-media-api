using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class NotificationConfig : IEntityTypeConfiguration<Notifications>
{
    public void Configure(EntityTypeBuilder<Notifications> builder)
    {
        builder.ToTable("notifications", "notifications");

        builder.HasKey(x => x.Id).HasName("pk_notifacations");
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.SenderId).HasColumnName("sender_id").IsRequired();
        builder.Property(x => x.ReciverId).HasColumnName("reciver_id").IsRequired();
        builder.Property(x => x.ThumbnailNoti).HasColumnName("thumbnail_noti");
        builder.Property(x => x.Target).HasColumnName("target").IsRequired().HasColumnType("enum.noti_target_enum");
        builder.Property(x => x.Action).HasColumnName("action").IsRequired().HasColumnType("enum.noti_action_enum");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("now()");
        builder.Property(x => x.ReadAt).HasColumnName("read_at");

        builder.HasOne(x => x.SenderNavi)
            .WithMany(u => u.SenderNotiNavi)
            .HasForeignKey(x => x.SenderId)
            .HasConstraintName("fk_noti_user_sender")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReciverNavi)
            .WithMany(u => u.ReciverNotiNavi)
            .HasForeignKey(x => x.ReciverId)
            .HasConstraintName("fk_noti_user_receiver")
            .OnDelete(DeleteBehavior.Cascade);
    }
}