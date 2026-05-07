using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini4rum.Migrations
{
    /// <inheritdoc />
    public partial class seed_admin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            migrationBuilder.Sql(
                $"""
                WITH inserted_account AS (
                    INSERT INTO auth.accounts (mail, pass, role, status, is_third_party)
                    VALUES ('admin@nhieuchuyen.local', '{adminPasswordHash}', 'admin'::enum.role_enum, 'active'::enum.status_account_enum, false)
                    RETURNING id
                )
                INSERT INTO "user".users (user_id, name, account_id)
                SELECT 'admin', 'Administrator', id
                FROM inserted_account;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "user".users WHERE user_id = 'admin';
                DELETE FROM auth.accounts WHERE mail = 'admin@nhieuchuyen.local';
                """);

        }
    }
}
