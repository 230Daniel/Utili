using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utili.Database.Migrations
{
    public partial class AutopurgeMessageIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_autopurge_messages_guild_id_channel_id_timestamp_is_pinned",
                table: "autopurge_messages",
                columns: new[] { "guild_id", "channel_id", "timestamp", "is_pinned" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_autopurge_messages_guild_id_channel_id_timestamp_is_pinned",
                table: "autopurge_messages");
        }
    }
}
