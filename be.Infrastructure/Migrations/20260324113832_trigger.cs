using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini4rum.Migrations
{
    /// <inheritdoc />
    public partial class trigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql("""
           CREATE OR REPLACE FUNCTION fn_update_total_follower()
           RETURNS TRIGGER AS $$
           BEGIN
           IF TG_OP = 'INSERT' THEN
           UPDATE "user".users
           SET total_follower = total_follower + 1
           WHERE id = NEW.followee_id;
           ELSIF TG_OP = 'DELETE' THEN
           UPDATE "user".users
           SET total_follower = total_follower - 1
           WHERE id = OLD.followee_id;
           END IF;
           RETURN NULL;
           END;
           $$ LANGUAGE plpgsql;
           """);

            migrationBuilder.Sql("""
           CREATE TRIGGER trg_update_total_follower
           AFTER INSERT OR DELETE ON engagement.follows
           FOR EACH ROW
           EXECUTE FUNCTION fn_update_total_follower();
           """);

            migrationBuilder.Sql("""
               CREATE OR REPLACE FUNCTION fn_update_total_following()
               RETURNS TRIGGER AS $$
               BEGIN
               IF TG_OP = 'INSERT' THEN
               UPDATE "user".users
               SET total_following = total_following + 1
               WHERE id = NEW.follower_id;
               ELSIF TG_OP = 'DELETE' THEN
               UPDATE "user".users
               SET total_following = total_following - 1
               WHERE id = OLD.follower_id;
               END IF;
               RETURN NULL;
               END;
               $$ LANGUAGE plpgsql;
               """);

            migrationBuilder.Sql("""
               CREATE TRIGGER trg_update_total_following
               AFTER INSERT OR DELETE ON engagement.follows
               FOR EACH ROW
               EXECUTE FUNCTION fn_update_total_following();
               """);

            migrationBuilder.Sql("""
               CREATE OR REPLACE FUNCTION fn_update_total_post()
               RETURNS TRIGGER AS $$
               BEGIN
               IF TG_OP = 'INSERT' THEN
               UPDATE "user".users
               SET total_post = total_post + 1
               WHERE id = NEW.user_id;
               ELSIF TG_OP = 'DELETE' THEN
               UPDATE "user".users
               SET total_post = total_post - 1
               WHERE id = OLD.user_id;
               END IF;
               RETURN NULL;
               END;
               $$ LANGUAGE plpgsql;
               """);

            migrationBuilder.Sql("""
               CREATE TRIGGER trg_update_total_post
               AFTER INSERT OR DELETE ON content.posts
               FOR EACH ROW
               EXECUTE FUNCTION fn_update_total_post();
               """);

            migrationBuilder.Sql("""
               CREATE OR REPLACE FUNCTION fn_update_total_react()
               RETURNS TRIGGER AS $$
               BEGIN
               IF TG_OP = 'INSERT' THEN
               UPDATE content.posts
               SET total_react = total_react + 1
               WHERE id = NEW.post_id;
               ELSIF TG_OP = 'DELETE' THEN
               UPDATE content.posts
               SET total_react = total_react - 1
               WHERE id = OLD.post_id;
               END IF;
               RETURN NULL;
               END;
               $$ LANGUAGE plpgsql;
               """);

            migrationBuilder.Sql("""
               CREATE TRIGGER trg_update_total_react
               AFTER INSERT OR DELETE ON engagement.react_posts
               FOR EACH ROW
               EXECUTE FUNCTION fn_update_total_react();
               """);

            migrationBuilder.Sql("""
               CREATE OR REPLACE FUNCTION fn_update_total_comment()
               RETURNS TRIGGER AS $$
               BEGIN
               IF TG_OP = 'INSERT' THEN
               UPDATE content.posts
               SET total_comment = total_comment + 1
               WHERE id = NEW.post_id;
               ELSIF TG_OP = 'DELETE' THEN
               UPDATE content.posts
               SET total_comment = total_comment - 1
               WHERE id = OLD.post_id;
               END IF;
               RETURN NULL;
               END;
               $$ LANGUAGE plpgsql;
               """);

            migrationBuilder.Sql("""
               CREATE TRIGGER trg_update_total_comment
               AFTER INSERT OR DELETE ON engagement.comments
               FOR EACH ROW
               EXECUTE FUNCTION fn_update_total_comment();
               """);

            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS trg_update_total_react ON engagement.react_posts;
DROP FUNCTION IF EXISTS fn_update_total_react();

CREATE OR REPLACE FUNCTION fn_update_total_react()
RETURNS TRIGGER AS $$
BEGIN
   IF TG_OP = 'INSERT' THEN
       IF NEW.type = 'like' THEN
           UPDATE content.posts SET total_like = total_like + 1 WHERE id = NEW.post_id;
       ELSIF NEW.type = 'dislike' THEN
           UPDATE content.posts SET total_dislike = total_dislike + 1 WHERE id = NEW.post_id;
       END IF;

   ELSIF TG_OP = 'DELETE' THEN
       IF OLD.type = 'like' THEN
           UPDATE content.posts SET total_like = total_like - 1 WHERE id = OLD.post_id;
       ELSIF OLD.type = 'dislike' THEN
           UPDATE content.posts SET total_dislike = total_dislike - 1 WHERE id = OLD.post_id;
       END IF;

   ELSIF TG_OP = 'UPDATE' THEN
       IF OLD.type = 'like' THEN
           UPDATE content.posts SET total_like = total_like - 1 WHERE id = OLD.post_id;
       ELSIF OLD.type = 'dislike' THEN
           UPDATE content.posts SET total_dislike = total_dislike - 1 WHERE id = OLD.post_id;
       END IF;

       -- Tăng count mới
       IF NEW.type = 'like' THEN
           UPDATE content.posts SET total_like = total_like + 1 WHERE id = NEW.post_id;
       ELSIF NEW.type = 'dislike' THEN
           UPDATE content.posts SET total_dislike = total_dislike + 1 WHERE id = NEW.post_id;
       END IF;
   END IF;

   RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_total_react
AFTER INSERT OR DELETE OR UPDATE ON engagement.react_posts
FOR EACH ROW
EXECUTE FUNCTION fn_update_total_react();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
