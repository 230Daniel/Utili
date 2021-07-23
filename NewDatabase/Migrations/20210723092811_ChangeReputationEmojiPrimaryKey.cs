using Microsoft.EntityFrameworkCore.Migrations;

namespace NewDatabase.Migrations
{
    public partial class ChangeReputationEmojiPrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_reputation_configuration_emoji_reputation_configurations_re",
                table: "reputation_configuration_emoji");

            migrationBuilder.DropPrimaryKey(
                name: "pk_reputation_configuration_emoji",
                table: "reputation_configuration_emoji");

            migrationBuilder.DropIndex(
                name: "ix_reputation_configuration_emoji_reputation_configuration_gui",
                table: "reputation_configuration_emoji");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "reputation_configuration_emoji");

            migrationBuilder.AlterColumn<decimal>(
                name: "reputation_configuration_guild_id",
                table: "reputation_configuration_emoji",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_reputation_configuration_emoji",
                table: "reputation_configuration_emoji",
                columns: new[] { "reputation_configuration_guild_id", "emoji" });

            migrationBuilder.AddForeignKey(
                name: "fk_reputation_configuration_emoji_reputation_configurations_re",
                table: "reputation_configuration_emoji",
                column: "reputation_configuration_guild_id",
                principalTable: "reputation_configurations",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_reputation_configuration_emoji_reputation_configurations_re",
                table: "reputation_configuration_emoji");

            migrationBuilder.DropPrimaryKey(
                name: "pk_reputation_configuration_emoji",
                table: "reputation_configuration_emoji");

            migrationBuilder.AlterColumn<decimal>(
                name: "reputation_configuration_guild_id",
                table: "reputation_configuration_emoji",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "reputation_configuration_emoji",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "pk_reputation_configuration_emoji",
                table: "reputation_configuration_emoji",
                columns: new[] { "guild_id", "emoji" });

            migrationBuilder.CreateIndex(
                name: "ix_reputation_configuration_emoji_reputation_configuration_gui",
                table: "reputation_configuration_emoji",
                column: "reputation_configuration_guild_id");

            migrationBuilder.AddForeignKey(
                name: "fk_reputation_configuration_emoji_reputation_configurations_re",
                table: "reputation_configuration_emoji",
                column: "reputation_configuration_guild_id",
                principalTable: "reputation_configurations",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
