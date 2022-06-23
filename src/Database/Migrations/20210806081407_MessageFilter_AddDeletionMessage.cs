using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class MessageFilter_AddDeletionMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "deletion_message",
                table: "message_filter_configurations",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deletion_message",
                table: "message_filter_configurations");
        }
    }
}
