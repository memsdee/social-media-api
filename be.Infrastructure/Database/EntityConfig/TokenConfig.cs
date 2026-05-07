using be.Domain.Entities;
using be.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class TokenConfig : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable("tokens", "auth");

        builder.HasKey(t => t.Id).HasName("pk_tokens");

        builder.Property(t => t.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(t => t.RefreshToken).HasColumnName("refresh_token").IsRequired().HasMaxLength(255);
        builder.Property(t => t.ExpiresAt).HasColumnName("expires_at").IsRequired().HasColumnType("timestamptz");
        builder.Property(t => t.AccountId).HasColumnName("account_id").IsRequired();

        builder.HasOne(t => t.AccountNavi)
            .WithMany(a => a.TokensNavi)
            .HasForeignKey(t => t.AccountId)
            .HasConstraintName("fk_account_token")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.AccountNavi.Status != StatusAccountEnum.deleted);
    }
}