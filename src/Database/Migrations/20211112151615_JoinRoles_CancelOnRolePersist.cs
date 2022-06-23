using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class JoinRoles_CancelOnRolePersist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "cancel_on_role_persist",
                table: "join_roles_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancel_on_role_persist",
                table: "join_roles_configurations");
        }
    }
}
