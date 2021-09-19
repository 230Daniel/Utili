using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NewDatabase.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "autopurge_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    timespan = table.Column<TimeSpan>(type: "interval", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_autopurge_configurations", x => new { x.guild_id, x.channel_id });
                });

            migrationBuilder.CreateTable(
                name: "autopurge_messages",
                columns: table => new
                {
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_bot = table.Column<bool>(type: "boolean", nullable: false),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_autopurge_messages", x => x.message_id);
                });

            migrationBuilder.CreateTable(
                name: "core_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    prefix = table.Column<string>(type: "text", nullable: true),
                    commands_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    non_command_channels = table.Column<decimal[]>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_core_configurations", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "inactive_role_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    immune_role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    threshold = table.Column<TimeSpan>(type: "interval", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    auto_kick = table.Column<bool>(type: "boolean", nullable: false),
                    auto_kick_threshold = table.Column<TimeSpan>(type: "interval", nullable: false),
                    default_last_action = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inactive_role_configurations", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "join_message_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    footer = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    thumbnail = table.Column<string>(type: "text", nullable: true),
                    icon = table.Column<string>(type: "text", nullable: true),
                    colour = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_join_message_configurations", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "join_roles_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    wait_for_verification = table.Column<bool>(type: "boolean", nullable: false),
                    join_roles = table.Column<decimal[]>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_join_roles_configurations", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "join_roles_pending_members",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    member_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    is_pending = table.Column<bool>(type: "boolean", nullable: false),
                    scheduled_for = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_join_roles_pending_members", x => new { x.guild_id, x.member_id });
                });

            migrationBuilder.CreateTable(
                name: "message_filter_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    reg_ex = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_filter_configurations", x => new { x.guild_id, x.channel_id });
                });

            migrationBuilder.CreateTable(
                name: "message_logs_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    deleted_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    edited_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    excluded_channels = table.Column<decimal[]>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_logs_configurations", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "message_logs_messages",
                columns: table => new
                {
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    author_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_logs_messages", x => x.message_id);
                });

            migrationBuilder.CreateTable(
                name: "message_pinning_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    pin_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    pin_messages = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_pinning_configurations", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "message_pinning_webhooks",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    webhook_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_pinning_webhooks", x => new { x.guild_id, x.channel_id });
                });

            migrationBuilder.CreateTable(
                name: "notice_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    delay = table.Column<TimeSpan>(type: "interval", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    footer = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    thumbnail = table.Column<string>(type: "text", nullable: true),
                    icon = table.Column<string>(type: "text", nullable: true),
                    colour = table.Column<long>(type: "bigint", nullable: false),
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    updated_from_dashboard = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notice_configurations", x => new { x.guild_id, x.channel_id });
                });

            migrationBuilder.CreateTable(
                name: "premium_slots",
                columns: table => new
                {
                    slot_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_premium_slots", x => x.slot_id);
                });

            migrationBuilder.CreateTable(
                name: "reputation_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reputation_configurations", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "reputation_members",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    member_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    reputation = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reputation_members", x => new { x.guild_id, x.member_id });
                });

            migrationBuilder.CreateTable(
                name: "role_linking_configurations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    linked_role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_linking_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_persist_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    excluded_roles = table.Column<decimal[]>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_persist_configurations", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    slots = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    customer_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "voice_link_channels",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    text_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voice_link_channels", x => new { x.guild_id, x.channel_id });
                });

            migrationBuilder.CreateTable(
                name: "voice_link_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    delete_channels = table.Column<bool>(type: "boolean", nullable: false),
                    channel_prefix = table.Column<string>(type: "text", nullable: true),
                    excluded_channels = table.Column<decimal[]>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voice_link_configurations", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "voice_role_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voice_role_configurations", x => new { x.guild_id, x.channel_id });
                });

            migrationBuilder.CreateTable(
                name: "vote_channel_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    emotes = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vote_channel_configurations", x => new { x.guild_id, x.channel_id });
                });

            migrationBuilder.CreateTable(
                name: "reputation_configuration_emoji",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    emoji = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false),
                    reputation_configuration_guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reputation_configuration_emoji", x => new { x.guild_id, x.emoji });
                    table.ForeignKey(
                        name: "fk_reputation_configuration_emoji_reputation_configurations_re",
                        column: x => x.reputation_configuration_guild_id,
                        principalTable: "reputation_configurations",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_message_logs_messages_timestamp",
                table: "message_logs_messages",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_reputation_configuration_emoji_reputation_configuration_gui",
                table: "reputation_configuration_emoji",
                column: "reputation_configuration_guild_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "autopurge_configurations");

            migrationBuilder.DropTable(
                name: "autopurge_messages");

            migrationBuilder.DropTable(
                name: "core_configurations");

            migrationBuilder.DropTable(
                name: "inactive_role_configurations");

            migrationBuilder.DropTable(
                name: "join_message_configurations");

            migrationBuilder.DropTable(
                name: "join_roles_configurations");

            migrationBuilder.DropTable(
                name: "join_roles_pending_members");

            migrationBuilder.DropTable(
                name: "message_filter_configurations");

            migrationBuilder.DropTable(
                name: "message_logs_configurations");

            migrationBuilder.DropTable(
                name: "message_logs_messages");

            migrationBuilder.DropTable(
                name: "message_pinning_configurations");

            migrationBuilder.DropTable(
                name: "message_pinning_webhooks");

            migrationBuilder.DropTable(
                name: "notice_configurations");

            migrationBuilder.DropTable(
                name: "premium_slots");

            migrationBuilder.DropTable(
                name: "reputation_configuration_emoji");

            migrationBuilder.DropTable(
                name: "reputation_members");

            migrationBuilder.DropTable(
                name: "role_linking_configurations");

            migrationBuilder.DropTable(
                name: "role_persist_configurations");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "voice_link_channels");

            migrationBuilder.DropTable(
                name: "voice_link_configurations");

            migrationBuilder.DropTable(
                name: "voice_role_configurations");

            migrationBuilder.DropTable(
                name: "vote_channel_configurations");

            migrationBuilder.DropTable(
                name: "reputation_configurations");
        }
    }
}
