using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class JoinMessage_CreateThread : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "create_thread",
                table: "join_message_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "thread_title",
                table: "join_message_configurations",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "create_thread",
                table: "join_message_configurations");

            migrationBuilder.DropColumn(
                name: "thread_title",
                table: "join_message_configurations");
        }
    }
}
