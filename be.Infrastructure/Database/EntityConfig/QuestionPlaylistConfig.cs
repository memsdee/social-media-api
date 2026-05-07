using be.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be.Infrastructure.Database.EntityConfig;

public class QuestionPlaylistConfig : IEntityTypeConfiguration<QuestionPlaylist>
{
    public void Configure(EntityTypeBuilder<QuestionPlaylist> builder)
    {
        builder.ToTable("question_playlists", "questions");

        builder.HasKey(x => x.Id).HasName("pk_question_playlists");

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Questions).HasColumnName("questions").HasColumnType("jsonb").IsRequired()
            .HasMaxLength(4000);
        builder.Property(x => x.CurrentIndex).HasColumnName("current_index").IsRequired();

        builder.HasOne(x => x.UserNavi)
            .WithOne(x => x.QuestionPlaylistNavi)
            .HasForeignKey<QuestionPlaylist>(x => x.UserId)
            .HasConstraintName("fk_playlists_users")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("ux_question_playlists_user_id");
    }
}