using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddShardDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shard_details",
                columns: table => new
                {
                    shard_id = table.Column<int>(type: "integer", nullable: false),
                    guilds = table.Column<int>(type: "integer", nullable: false),
                    heartbeat = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shard_details", x => x.shard_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shard_details");
        }
    }
}
