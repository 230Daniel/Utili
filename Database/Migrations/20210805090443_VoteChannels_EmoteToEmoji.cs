using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class VoteChannels_EmoteToEmoji : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "emotes",
                table: "vote_channel_configurations",
                newName: "emojis");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "emojis",
                table: "vote_channel_configurations",
                newName: "emotes");
        }
    }
}
