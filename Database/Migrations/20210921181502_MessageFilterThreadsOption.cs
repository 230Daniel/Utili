using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class MessageFilterThreadsOption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "enforce_in_threads",
                table: "message_filter_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "enforce_in_threads",
                table: "message_filter_configurations");
        }
    }
}
