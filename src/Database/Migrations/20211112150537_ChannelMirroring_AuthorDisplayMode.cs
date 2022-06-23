using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class ChannelMirroring_AuthorDisplayMode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "author_display_mode",
                table: "channel_mirroring_configurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "author_display_mode",
                table: "channel_mirroring_configurations");
        }
    }
}
