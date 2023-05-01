using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utili.Database.Migrations
{
    public partial class ClassForEachBulkDeletedMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "messages",
                table: "message_logs_bulk_deleted_messages");

            migrationBuilder.CreateTable(
                name: "message_logs_bulk_deleted_message",
                columns: table => new
                {
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    username = table.Column<string>(type: "text", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    message_logs_bulk_deleted_messages_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_logs_bulk_deleted_message", x => x.message_id);
                    table.ForeignKey(
                        name: "fk_message_logs_bulk_deleted_message_message_logs_bulk_deleted",
                        column: x => x.message_logs_bulk_deleted_messages_id,
                        principalTable: "message_logs_bulk_deleted_messages",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_message_logs_bulk_deleted_message_message_logs_bulk_deleted",
                table: "message_logs_bulk_deleted_message",
                column: "message_logs_bulk_deleted_messages_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "message_logs_bulk_deleted_message");

            migrationBuilder.AddColumn<string[]>(
                name: "messages",
                table: "message_logs_bulk_deleted_messages",
                type: "text[]",
                nullable: true);
        }
    }
}
