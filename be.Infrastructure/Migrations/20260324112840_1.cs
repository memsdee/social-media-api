using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mini4rum.Migrations
{
    /// <inheritdoc />
    public partial class _1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "logging");

            migrationBuilder.EnsureSchema(
                name: "engagement");

            migrationBuilder.EnsureSchema(
                name: "chat");

            migrationBuilder.EnsureSchema(
                name: "support");

            migrationBuilder.EnsureSchema(
                name: "notifications");

            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.EnsureSchema(
                name: "questions");

            migrationBuilder.EnsureSchema(
                name: "user");

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
                .Annotation("Npgsql:Enum:enum.type_message_enum", "text,image,video,system");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pass = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    mail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    role = table.Column<int>(type: "enum.role_enum", nullable: false),
                    score = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    is_third_party = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    create_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    status = table.Column<int>(type: "enum.status_account_enum", nullable: false, defaultValueSql: "'active'::enum.status_account_enum")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                    table.CheckConstraint("ck_mail_format", "mail ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");
                    table.CheckConstraint("ck_pass_lenght", "char_length(pass) > 5");
                    table.CheckConstraint("ck_pass_or_third_party", "is_third_party OR (pass IS NOT NULL AND char_length(pass) > 5)");
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                schema: "chat",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_public = table.Column<Guid>(type: "uuid", nullable: false),
                    last_message = table.Column<string>(type: "text", maxLength: 70, nullable: true),
                    creator_id = table.Column<short>(type: "smallint", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<int>(type: "enum.type_conversation_enum", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    key_participant = table.Column<string>(type: "text", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversation", x => x.id);
                    table.CheckConstraint("ck_last_message_length", "char_length(last_message) <= 70");
                    table.CheckConstraint("ck_single_conversation_key_participants", "type != 'single' OR (type = 'single' AND key_participant IS NOT NULL)");
                });

            migrationBuilder.CreateTable(
                name: "questions",
                schema: "questions",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_questions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reason_report_post",
                schema: "engagement",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report", x => x.id);
                    table.CheckConstraint("ck_check_code", "code >= 0");
                });

            migrationBuilder.CreateTable(
                name: "third_party_logins",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    provider = table.Column<int>(type: "enum.third_party_login_enum", nullable: false),
                    provider_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    account_id = table.Column<short>(type: "smallint", nullable: false),
                    mail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_third_party_login", x => x.id);
                    table.CheckConstraint("ck_mail_format", "mail ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");
                    table.ForeignKey(
                        name: "fk_third_party_login_account",
                        column: x => x.account_id,
                        principalSchema: "auth",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    refresh_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    account_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_token",
                        column: x => x.account_id,
                        principalSchema: "auth",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    avatar = table.Column<Guid>(type: "uuid", nullable: true),
                    bio = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    total_follower = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    total_following = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    total_post = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    account_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_user_id_type", "char_length(user_id) BETWEEN 3 AND 50 AND user_id ~ '^[A-Za-z0-9_]+$'");
                    table.CheckConstraint("ck_user_name_type", "char_length(name) BETWEEN 3 AND 50 AND name !~ '^\\s' AND name !~ '\\s$' AND name !~ '\\s{2,}'");
                    table.CheckConstraint("ck_users_total_non_negative", "total_follower >= 0 AND total_following >= 0 AND total_post >= 0");
                    table.ForeignKey(
                        name: "fk_users_accounts",
                        column: x => x.account_id,
                        principalSchema: "auth",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admin_del_account_logs",
                schema: "logging",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    target_id = table.Column<short>(type: "smallint", nullable: false),
                    admin_id = table.Column<short>(type: "smallint", nullable: true),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_del_account_logs", x => x.id);
                    table.CheckConstraint("ck_admin_del_account_log_fk_positive", "target_id > 0 AND (admin_id >= 0 OR admin_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_accounts_admin_del_account_logs",
                        column: x => x.admin_id,
                        principalSchema: "auth",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_accounts_target_del_account_navi",
                        column: x => x.target_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversation_participants",
                schema: "chat",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    conversation_id = table.Column<short>(type: "smallint", nullable: false),
                    user_id = table.Column<short>(type: "smallint", nullable: false),
                    unread_count = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversation_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_conversation_participants_conversation_id",
                        column: x => x.conversation_id,
                        principalSchema: "chat",
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_conversation_participants_user_id",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "feedbacks",
                schema: "support",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content = table.Column<string>(type: "text", maxLength: 5000, nullable: false),
                    user_id = table.Column<short>(type: "smallint", nullable: false),
                    create_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feedback", x => x.id);
                    table.CheckConstraint("ck_content_length", "char_length(content) >= 20");
                    table.ForeignKey(
                        name: "fk_feedback_user",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "follows",
                schema: "engagement",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    follower_id = table.Column<short>(type: "smallint", nullable: false),
                    followee_id = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_follow", x => x.id);
                    table.CheckConstraint("ck_follow_no_self", "\"follower_id\" <> \"followee_id\"");
                    table.ForeignKey(
                        name: "fk_following_user",
                        column: x => x.followee_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_follwer_user",
                        column: x => x.follower_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "chat",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    conversation_id = table.Column<short>(type: "smallint", nullable: false),
                    sender_id = table.Column<short>(type: "smallint", nullable: false),
                    content = table.Column<string>(type: "text", maxLength: 5000, nullable: false),
                    type = table.Column<int>(type: "enum.type_message_enum", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                    table.CheckConstraint("ck_message_content_length", "char_length(content) <= 5000");
                    table.ForeignKey(
                        name: "fk_messages_conversation_id",
                        column: x => x.conversation_id,
                        principalSchema: "chat",
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_messages_sender_id",
                        column: x => x.sender_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "notifications",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sender_id = table.Column<short>(type: "smallint", nullable: false),
                    reciver_id = table.Column<short>(type: "smallint", nullable: false),
                    thumbnail_noti = table.Column<Guid>(type: "uuid", nullable: true),
                    target = table.Column<int>(type: "enum.noti_target_enum", nullable: false),
                    action = table.Column<int>(type: "enum.noti_action_enum", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifacations", x => x.id);
                    table.ForeignKey(
                        name: "fk_noti_user_receiver",
                        column: x => x.reciver_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_noti_user_sender",
                        column: x => x.sender_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_public = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    total_comment = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    total_like = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    total_dislike = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    user_id = table.Column<short>(type: "smallint", nullable: false),
                    creat_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    score = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    score_report = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    status = table.Column<int>(type: "enum.status_post_enum", nullable: false, defaultValueSql: "'active'::enum.status_post_enum")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_user",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "question_playlists",
                schema: "questions",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<short>(type: "smallint", nullable: false),
                    questions = table.Column<string>(type: "jsonb", maxLength: 4000, nullable: false),
                    current_index = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_playlists", x => x.id);
                    table.ForeignKey(
                        name: "fk_playlists_users",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_question_logs",
                schema: "logging",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<short>(type: "smallint", nullable: false),
                    question_id = table.Column<short>(type: "smallint", nullable: false),
                    total_questions = table.Column<short>(type: "smallint", nullable: false),
                    show_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_question_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_question_logs_questions",
                        column: x => x.question_id,
                        principalSchema: "questions",
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_question_logs_users",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userid_change_log",
                schema: "logging",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    old_userid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    new_userid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    user_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_userid_change_log", x => x.id);
                    table.CheckConstraint("ck_new_userid_type", "char_length(new_userid) BETWEEN 3 AND 50 AND new_userid ~ '^[A-Za-z0-9_]+$'");
                    table.CheckConstraint("ck_old_userid_type", "char_length(old_userid) BETWEEN 3 AND 50 AND old_userid ~ '^[A-Za-z0-9_]+$'");
                    table.ForeignKey(
                        name: "fk_userid_change_log_user",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "username_change_log",
                schema: "logging",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    old_username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    new_username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    change_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    user_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_username_change_log", x => x.id);
                    table.CheckConstraint("ck_new_username_type", "char_length(new_username) BETWEEN 3 AND 50 AND new_username !~ '^\\s' AND new_username !~ '\\s$' AND new_username !~ '\\s{2,}'");
                    table.CheckConstraint("ck_old_username_type", "char_length(old_username) BETWEEN 3 AND 50 AND old_username !~ '^\\s' AND old_username !~ '\\s$' AND old_username !~ '\\s{2,}'");
                    table.ForeignKey(
                        name: "fk_username_change_log_user",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message_reads",
                schema: "chat",
                columns: table => new
                {
                    message_id = table.Column<short>(type: "smallint", nullable: false),
                    user_id = table.Column<short>(type: "smallint", nullable: false),
                    conversation_id = table.Column<short>(type: "smallint", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_reads", x => new { x.user_id, x.message_id });
                    table.ForeignKey(
                        name: "fk_messagereads_user",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_readmessage_conversation",
                        column: x => x.conversation_id,
                        principalSchema: "chat",
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_readmessage_message",
                        column: x => x.message_id,
                        principalSchema: "chat",
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admin_del_post_logs",
                schema: "logging",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    post_id = table.Column<short>(type: "smallint", nullable: false),
                    admin_id = table.Column<short>(type: "smallint", nullable: true),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_del_post_logs", x => x.id);
                    table.CheckConstraint("ck_admin_del_post_log_fk_positive", "post_id > 0 AND (admin_id >= 0 OR admin_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_accounts_admin_del_post_logs",
                        column: x => x.admin_id,
                        principalSchema: "auth",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_accounts_post_del_post_navi",
                        column: x => x.post_id,
                        principalSchema: "content",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                schema: "engagement",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_public = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    user_id = table.Column<short>(type: "smallint", nullable: false),
                    post_id = table.Column<short>(type: "smallint", nullable: false),
                    create_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comment", x => x.id);
                    table.CheckConstraint("ck_content_length", "char_length(content) >= 1");
                    table.ForeignKey(
                        name: "fk_comment_post",
                        column: x => x.post_id,
                        principalSchema: "content",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_comment_user",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "noti_reacts",
                schema: "notifications",
                columns: table => new
                {
                    noti_id = table.Column<short>(type: "smallint", nullable: false),
                    post_id = table.Column<short>(type: "smallint", nullable: false),
                    type = table.Column<int>(type: "enum.react_enum", nullable: false),
                    preview_content = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_noti_reacts", x => x.noti_id);
                    table.ForeignKey(
                        name: "fk_noti",
                        column: x => x.noti_id,
                        principalSchema: "notifications",
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notireactpost_post",
                        column: x => x.post_id,
                        principalSchema: "content",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_images",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    image = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "enum.image_enum", nullable: false),
                    group_id = table.Column<short>(type: "smallint", nullable: true),
                    post_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_image_post",
                        column: x => x.post_id,
                        principalSchema: "content",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "react_posts",
                schema: "engagement",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "enum.react_enum", nullable: false),
                    user_id = table.Column<short>(type: "smallint", nullable: false),
                    post_id = table.Column<short>(type: "smallint", nullable: false),
                    creat_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_react", x => x.id);
                    table.ForeignKey(
                        name: "fk_react_post",
                        column: x => x.post_id,
                        principalSchema: "content",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_react_user",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_report_posts",
                schema: "engagement",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reporter_id = table.Column<short>(type: "smallint", nullable: false),
                    reported_post = table.Column<short>(type: "smallint", nullable: false),
                    report_code = table.Column<short>(type: "smallint", nullable: false),
                    other_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "enum.status_report_post_enum", nullable: false, defaultValueSql: "'pending'::enum.status_report_post_enum"),
                    create_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_reports_posts",
                        column: x => x.reported_post,
                        principalSchema: "content",
                        principalTable: "posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reports_report_reasons",
                        column: x => x.report_code,
                        principalSchema: "engagement",
                        principalTable: "reason_report_post",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reports_users",
                        column: x => x.reporter_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "noti_cmts",
                schema: "notifications",
                columns: table => new
                {
                    noti_id = table.Column<short>(type: "smallint", nullable: false),
                    post_id = table.Column<short>(type: "smallint", nullable: false),
                    cmt_id = table.Column<short>(type: "smallint", nullable: false),
                    preview = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cmts", x => x.noti_id);
                    table.ForeignKey(
                        name: "fk_noti",
                        column: x => x.noti_id,
                        principalSchema: "notifications",
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_noticmt_cmt",
                        column: x => x.cmt_id,
                        principalSchema: "engagement",
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_noticmt_post",
                        column: x => x.post_id,
                        principalSchema: "content",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admin_resolve_report_log",
                schema: "logging",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_id = table.Column<short>(type: "smallint", nullable: false),
                    admin_id = table.Column<short>(type: "smallint", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_resolve_report_log", x => x.id);
                    table.CheckConstraint("ck_admin_resolve_report_log_fk_positive", "report_id > 0 AND admin_id > 0");
                    table.ForeignKey(
                        name: "fk_admin_resolve_report_log_report",
                        column: x => x.report_id,
                        principalSchema: "engagement",
                        principalTable: "user_report_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_admin_resolve_report_log_user",
                        column: x => x.admin_id,
                        principalSchema: "auth",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "uq_mail",
                schema: "auth",
                table: "accounts",
                column: "mail",
                unique: true,
                filter: "status = 'active'");

            migrationBuilder.CreateIndex(
                name: "IX_admin_del_account_logs_admin_id",
                schema: "logging",
                table: "admin_del_account_logs",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "uq_target_admin",
                schema: "logging",
                table: "admin_del_account_logs",
                columns: new[] { "target_id", "admin_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_del_post_logs_admin_id",
                schema: "logging",
                table: "admin_del_post_logs",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "uq_post_admin",
                schema: "logging",
                table: "admin_del_post_logs",
                columns: new[] { "post_id", "admin_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_resolve_report_log_admin_id",
                schema: "logging",
                table: "admin_resolve_report_log",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "uq_report_admin",
                schema: "logging",
                table: "admin_resolve_report_log",
                columns: new[] { "report_id", "admin_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comments_post_id",
                schema: "engagement",
                table: "comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_user_id",
                schema: "engagement",
                table: "comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_participants_conversation_id",
                schema: "chat",
                table: "conversation_participants",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ux_conversation_participants_conversation_id_user_id",
                schema: "chat",
                table: "conversation_participants",
                columns: new[] { "user_id", "conversation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_conversations_key_participants",
                schema: "chat",
                table: "conversations",
                column: "key_participant",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_user_id",
                schema: "support",
                table: "feedbacks",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_follows_followee_id",
                schema: "engagement",
                table: "follows",
                column: "followee_id");

            migrationBuilder.CreateIndex(
                name: "uq_follows_follower_followee",
                schema: "engagement",
                table: "follows",
                columns: new[] { "follower_id", "followee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_message_reads_conversation_id",
                schema: "chat",
                table: "message_reads",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_reads_message_id",
                schema: "chat",
                table: "message_reads",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_conversation_id",
                schema: "chat",
                table: "messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_sender_id",
                schema: "chat",
                table: "messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_noti_cmts_cmt_id",
                schema: "notifications",
                table: "noti_cmts",
                column: "cmt_id");

            migrationBuilder.CreateIndex(
                name: "IX_noti_cmts_post_id",
                schema: "notifications",
                table: "noti_cmts",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_noti_reacts_post_id",
                schema: "notifications",
                table: "noti_reacts",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_reciver_id",
                schema: "notifications",
                table: "notifications",
                column: "reciver_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_sender_id",
                schema: "notifications",
                table: "notifications",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_images_post_id",
                schema: "content",
                table: "post_images",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_posts_user_id",
                schema: "content",
                table: "posts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_question_playlists_user_id",
                schema: "questions",
                table: "question_playlists",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_content",
                schema: "questions",
                table: "questions",
                column: "content",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_react_posts_post_id",
                schema: "engagement",
                table: "react_posts",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "uq_react_user_post",
                schema: "engagement",
                table: "react_posts",
                columns: new[] { "user_id", "post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_code",
                schema: "engagement",
                table: "reason_report_post",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_third_party_logins_account_id",
                schema: "auth",
                table: "third_party_logins",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "uq_provider_provider_id",
                schema: "auth",
                table: "third_party_logins",
                columns: new[] { "provider", "provider_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tokens_account_id",
                schema: "auth",
                table: "tokens",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_question_logs_question_id",
                schema: "logging",
                table: "user_question_logs",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_question_logs_user_id",
                schema: "logging",
                table: "user_question_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_report_posts_report_code",
                schema: "engagement",
                table: "user_report_posts",
                column: "report_code");

            migrationBuilder.CreateIndex(
                name: "IX_user_report_posts_reported_post",
                schema: "engagement",
                table: "user_report_posts",
                column: "reported_post");

            migrationBuilder.CreateIndex(
                name: "uq_user_report_once_per_reason",
                schema: "engagement",
                table: "user_report_posts",
                columns: new[] { "reporter_id", "reported_post", "report_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_userid_change_log_user_id",
                schema: "logging",
                table: "userid_change_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_username_change_log_user_id",
                schema: "logging",
                table: "username_change_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_account_id",
                schema: "user",
                table: "users",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_userid",
                schema: "user",
                table: "users",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_del_account_logs",
                schema: "logging");

            migrationBuilder.DropTable(
                name: "admin_del_post_logs",
                schema: "logging");

            migrationBuilder.DropTable(
                name: "admin_resolve_report_log",
                schema: "logging");

            migrationBuilder.DropTable(
                name: "conversation_participants",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "feedbacks",
                schema: "support");

            migrationBuilder.DropTable(
                name: "follows",
                schema: "engagement");

            migrationBuilder.DropTable(
                name: "message_reads",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "noti_cmts",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "noti_reacts",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "post_images",
                schema: "content");

            migrationBuilder.DropTable(
                name: "question_playlists",
                schema: "questions");

            migrationBuilder.DropTable(
                name: "react_posts",
                schema: "engagement");

            migrationBuilder.DropTable(
                name: "third_party_logins",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_question_logs",
                schema: "logging");

            migrationBuilder.DropTable(
                name: "userid_change_log",
                schema: "logging");

            migrationBuilder.DropTable(
                name: "username_change_log",
                schema: "logging");

            migrationBuilder.DropTable(
                name: "user_report_posts",
                schema: "engagement");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "comments",
                schema: "engagement");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "questions",
                schema: "questions");

            migrationBuilder.DropTable(
                name: "reason_report_post",
                schema: "engagement");

            migrationBuilder.DropTable(
                name: "conversations",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "posts",
                schema: "content");

            migrationBuilder.DropTable(
                name: "users",
                schema: "user");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "auth");
        }
    }
}
