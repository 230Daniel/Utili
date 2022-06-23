using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Utili.Database.Migrations
{
    public partial class Changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "added_from_dashboard",
                table: "autopurge_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "inactive_role_members",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    member_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    last_action = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inactive_role_members", x => new { x.guild_id, x.member_id });
                });

            migrationBuilder.CreateTable(
                name: "role_persist_members",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    member_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    roles = table.Column<decimal[]>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_persist_members", x => new { x.guild_id, x.member_id });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inactive_role_members");

            migrationBuilder.DropTable(
                name: "role_persist_members");

            migrationBuilder.DropColumn(
                name: "added_from_dashboard",
                table: "autopurge_configurations");
        }
    }
}
