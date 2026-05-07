using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class OutboxConfig : IEntityTypeConfiguration<Outbox>
{
    public void Configure(EntityTypeBuilder<Outbox> builder)
    {
        builder.ToTable("outbox", "public");

        builder.HasKey(x => x.Id).HasName("pk_outbox");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.Topic).HasColumnName("topic").IsRequired().HasColumnType("enum.outbox_topic_enum");
        builder.Property(e => e.Payload).HasColumnName("payload").IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("now()");
    }
}