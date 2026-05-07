using be.Domain.Entities;
using be.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class AccountConfig : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", "auth");

        builder.HasKey(e => e.Id).HasName("pk_accounts");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.Pass).HasColumnName("pass").HasMaxLength(255);
        builder.Property(e => e.Mail).HasColumnName("mail").HasMaxLength(255);
        builder.Property(e => e.Score).HasColumnName("score").IsRequired().HasDefaultValue(0);
        builder.Property(e => e.Role).HasColumnName("role").IsRequired().HasColumnType("enum.role_enum");
        builder.Property(e => e.IsThirdParty).HasColumnName("is_third_party").IsRequired().HasDefaultValue(false);
        builder.Property(e => e.CreatAt).HasColumnName("create_at").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.Status).HasColumnName("status").IsRequired()
            .HasDefaultValueSql("'active'::enum.status_account_enum").HasColumnType("enum.status_account_enum");

        builder.HasIndex(e => e.Mail, "uq_mail").IsUnique().HasFilter("status = 'active'");

        builder.HasQueryFilter(e => e.Status == StatusAccountEnum.active);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_pass_lenght", "char_length(pass) > 5");
            t.HasCheckConstraint("ck_mail_format", "mail ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");
        });

        builder.ToTable(t => t.HasCheckConstraint("ck_pass_or_third_party",
            "is_third_party OR (pass IS NOT NULL AND char_length(pass) > 5)"));
    }
}