using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class ThirdPartyLoginConfig : IEntityTypeConfiguration<ThirdPartyLogin>
{
    public void Configure(EntityTypeBuilder<ThirdPartyLogin> builder)
    {
        builder.ToTable("third_party_logins", "auth");

        builder.HasKey(u => u.Id).HasName("pk_third_party_login");

        builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(p => p.Provider).HasColumnName("provider").IsRequired()
            .HasColumnType("enum.third_party_login_enum");
        builder.Property(p => p.ProviderId).HasColumnName("provider_id").IsRequired().HasMaxLength(100);
        builder.Property(p => p.AccountId).HasColumnName("account_id").IsRequired().HasColumnType("smallint");
        builder.Property(p => p.Mail).HasColumnName("mail").IsRequired().HasMaxLength(255);

        builder.HasOne(tp => tp.AccountNavi)
            .WithMany(a => a.ThirdPartyLoginsNavi)
            .HasForeignKey(tp => tp.AccountId)
            .HasConstraintName("fk_third_party_login_account")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(tp => new { tp.Provider, tp.ProviderId }).IsUnique()
            .HasDatabaseName("uq_provider_provider_id");
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_mail_format", "mail ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");
        });
    }
}