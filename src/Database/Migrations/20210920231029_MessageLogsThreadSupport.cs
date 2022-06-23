using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class MessageLogsThreadSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "text_channel_id",
                table: "message_logs_messages",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "log_threads",
                table: "message_logs_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "text_channel_id",
                table: "message_logs_messages");

            migrationBuilder.DropColumn(
                name: "log_threads",
                table: "message_logs_configurations");
        }
    }
}
