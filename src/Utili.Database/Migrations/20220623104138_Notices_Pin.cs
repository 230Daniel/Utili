using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utili.Database.Migrations
{
    public partial class Notices_Pin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "pin",
                table: "notice_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pin",
                table: "notice_configurations");
        }
    }
}
