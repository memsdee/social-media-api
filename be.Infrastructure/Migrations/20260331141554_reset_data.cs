using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini4rum.Migrations
{
    /// <inheritdoc />
    public partial class reset_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    rec RECORD;
                BEGIN
                    FOR rec IN
                        SELECT schemaname, tablename
                        FROM pg_tables
                        WHERE schemaname IN ('logging', 'social', 'engagement', 'content', 'user', 'auth', 'questions')
                          AND tablename <> '__EFMigrationsHistory'
                    LOOP
                        EXECUTE format('TRUNCATE TABLE %I.%I RESTART IDENTITY CASCADE', rec.schemaname, rec.tablename);
                    END LOOP;
                END $$;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
