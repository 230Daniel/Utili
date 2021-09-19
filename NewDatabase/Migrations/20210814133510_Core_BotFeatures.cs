using Microsoft.EntityFrameworkCore.Migrations;

namespace NewDatabase.Migrations
{
    public partial class Core_BotFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bot_features",
                table: "core_configurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bot_features",
                table: "core_configurations");
        }
    }
}
