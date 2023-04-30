using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utili.Database.Migrations
{
    public partial class MessageLogsBulkDeletedMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shard_details");

            migrationBuilder.CreateTable(
                name: "message_logs_bulk_deleted_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    messages_deleted = table.Column<int>(type: "integer", nullable: false),
                    messages_logged = table.Column<int>(type: "integer", nullable: false),
                    messages = table.Column<string[]>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_logs_bulk_deleted_messages", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "message_logs_bulk_deleted_messages");

            migrationBuilder.CreateTable(
                name: "shard_details",
                columns: table => new
                {
                    shard_id = table.Column<int>(type: "integer", nullable: false),
                    guilds = table.Column<int>(type: "integer", nullable: false),
                    heartbeat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shard_details", x => x.shard_id);
                });
        }
    }
}
