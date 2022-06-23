using Microsoft.EntityFrameworkCore.Migrations;

namespace Utili.Database.Migrations
{
    public partial class FixRoleLinkingConfigurations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "role_id",
                table: "role_linking_configurations",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role_id",
                table: "role_linking_configurations");
        }
    }
}
