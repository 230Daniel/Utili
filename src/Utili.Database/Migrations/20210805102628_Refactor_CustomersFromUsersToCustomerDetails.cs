using Microsoft.EntityFrameworkCore.Migrations;

namespace Utili.Database.Migrations
{
    public partial class Refactor_CustomersFromUsersToCustomerDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_id",
                table: "users");

            migrationBuilder.CreateTable(
                name: "customer_details",
                columns: table => new
                {
                    customer_id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_details", x => x.customer_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_details");

            migrationBuilder.AddColumn<string>(
                name: "customer_id",
                table: "users",
                type: "text",
                nullable: true);
        }
    }
}
