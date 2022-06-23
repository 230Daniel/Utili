using Microsoft.EntityFrameworkCore.Migrations;

namespace Utili.Database.Migrations
{
    public partial class AddChannelMirroring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channel_mirroring_configurations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    destination_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    webhook_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_mirroring_configurations", x => new { x.guild_id, x.channel_id });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "channel_mirroring_configurations");
        }
    }
}
