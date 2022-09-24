using System.Linq;
using Disqord;
using Disqord.Gateway;

namespace Utili.Bot.Extensions;

public static class RoleExtensions
{
    public static bool CanBeManaged(this IRole role)
    {
        IGuild guild = (role.Client as DiscordClientBase).GetGuild(role.GuildId);
        if (!guild.BotHasPermissions(Permissions.ManageRoles)) return false;
        if (role.IsManaged) return false;

        // The higher the position the higher the role in the hierarchy
        var bot = guild.GetCurrentMember();
        var botRoles = bot.GetRoles().Values;
        var botHighestPosition = botRoles.Max(x => x.Position);
        var botHighestRole = botRoles.Where(x => x.Position == botHighestPosition).OrderByDescending(x => x.Id).First();

        return botHighestRole.Position > role.Position || (botHighestRole.Position == role.Position && botHighestRole.Id < role.Id);
    }
}
