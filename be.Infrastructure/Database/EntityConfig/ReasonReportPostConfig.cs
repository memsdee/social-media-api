using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class ReasonReportPostConfig : IEntityTypeConfiguration<ReasonReportPost>
{
    public void Configure(EntityTypeBuilder<ReasonReportPost> builder)
    {
        builder.ToTable("reason_report_post", "engagement");

        builder.HasKey(e => e.Id).HasName("pk_report");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.Code).HasColumnName("code").IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(500);

        builder.HasIndex(e => e.Code, "uq_code").IsUnique();

        builder.ToTable(t => { t.HasCheckConstraint("ck_check_code", "code >= 0"); });
    }
}