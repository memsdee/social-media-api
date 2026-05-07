using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using be.Domain.Enums;

#nullable disable

namespace mini4rum.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "create_at",
                schema: "support",
                table: "feedbacks");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:enum.image_enum", "after,before,normal")
                .Annotation("Npgsql:Enum:enum.noti_action_enum", "react,comment,follow")
                .Annotation("Npgsql:Enum:enum.noti_target_enum", "post,user")
                .Annotation("Npgsql:Enum:enum.outbox_topic_enum", "post_del_by_score,del_account,follow,notification,un_follow,update_avatar,conversation,mark_as_read_message,message,mark_as_read_notification,comment,noti_comment,react_post,noti_react_post,post,report_post")
                .Annotation("Npgsql:Enum:enum.react_enum", "like,dislike")
                .Annotation("Npgsql:Enum:enum.role_enum", "admin,user")
                .Annotation("Npgsql:Enum:enum.status_account_enum", "active,banned,deleted")
                .Annotation("Npgsql:Enum:enum.status_post_enum", "active,deleted")
                .Annotation("Npgsql:Enum:enum.status_report_post_enum", "pending,resolved,rejected")
                .Annotation("Npgsql:Enum:enum.third_party_login_enum", "google")
                .Annotation("Npgsql:Enum:enum.type_conversation_enum", "single,group")
                .Annotation("Npgsql:Enum:enum.type_message_enum", "text,image,video,system")
                .OldAnnotation("Npgsql:Enum:enum.image_enum", "after,before,normal")
                .OldAnnotation("Npgsql:Enum:enum.noti_action_enum", "react,comment,follow")
                .OldAnnotation("Npgsql:Enum:enum.noti_target_enum", "post,user")
                .OldAnnotation("Npgsql:Enum:enum.react_enum", "like,dislike")
                .OldAnnotation("Npgsql:Enum:enum.role_enum", "admin,user")
                .OldAnnotation("Npgsql:Enum:enum.status_account_enum", "active,banned,deleted")
                .OldAnnotation("Npgsql:Enum:enum.status_post_enum", "active,deleted")
                .OldAnnotation("Npgsql:Enum:enum.status_report_post_enum", "pending,resolved,rejected")
                .OldAnnotation("Npgsql:Enum:enum.third_party_login_enum", "google")
                .OldAnnotation("Npgsql:Enum:enum.type_conversation_enum", "single,group")
                .OldAnnotation("Npgsql:Enum:enum.type_message_enum", "text,image,video,system");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                schema: "support",
                table: "feedbacks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<long>(
                name: "key_participant",
                schema: "chat",
                table: "conversations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "outbox",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    topic = table.Column<OutboxTopicEnum>(type: "enum.outbox_topic_enum", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_post_score_active_only",
                schema: "content",
                table: "posts",
                column: "score",
                filter: "\"status\" = 'active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "idx_post_score_active_only",
                schema: "content",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "support",
                table: "feedbacks");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:enum.image_enum", "after,before,normal")
                .Annotation("Npgsql:Enum:enum.noti_action_enum", "react,comment,follow")
                .Annotation("Npgsql:Enum:enum.noti_target_enum", "post,user")
                .Annotation("Npgsql:Enum:enum.react_enum", "like,dislike")
                .Annotation("Npgsql:Enum:enum.role_enum", "admin,user")
                .Annotation("Npgsql:Enum:enum.status_account_enum", "active,banned,deleted")
                .Annotation("Npgsql:Enum:enum.status_post_enum", "active,deleted")
                .Annotation("Npgsql:Enum:enum.status_report_post_enum", "pending,resolved,rejected")
                .Annotation("Npgsql:Enum:enum.third_party_login_enum", "google")
                .Annotation("Npgsql:Enum:enum.type_conversation_enum", "single,group")
                .Annotation("Npgsql:Enum:enum.type_message_enum", "text,image,video,system")
                .OldAnnotation("Npgsql:Enum:enum.image_enum", "after,before,normal")
                .OldAnnotation("Npgsql:Enum:enum.noti_action_enum", "react,comment,follow")
                .OldAnnotation("Npgsql:Enum:enum.noti_target_enum", "post,user")
                .OldAnnotation("Npgsql:Enum:enum.outbox_topic_enum", "post_del_by_score,del_account,follow,notification,un_follow,update_avatar,conversation,mark_as_read_message,message,mark_as_read_notification,comment,noti_comment,react_post,noti_react_post,post,report_post")
                .OldAnnotation("Npgsql:Enum:enum.react_enum", "like,dislike")
                .OldAnnotation("Npgsql:Enum:enum.role_enum", "admin,user")
                .OldAnnotation("Npgsql:Enum:enum.status_account_enum", "active,banned,deleted")
                .OldAnnotation("Npgsql:Enum:enum.status_post_enum", "active,deleted")
                .OldAnnotation("Npgsql:Enum:enum.status_report_post_enum", "pending,resolved,rejected")
                .OldAnnotation("Npgsql:Enum:enum.third_party_login_enum", "google")
                .OldAnnotation("Npgsql:Enum:enum.type_conversation_enum", "single,group")
                .OldAnnotation("Npgsql:Enum:enum.type_message_enum", "text,image,video,system");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "create_at",
                schema: "support",
                table: "feedbacks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AlterColumn<string>(
                name: "key_participant",
                schema: "chat",
                table: "conversations",
                type: "text",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
