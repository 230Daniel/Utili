using System.Linq;
using Disqord;
using Disqord.Gateway;

namespace Utili.Extensions
{
    public static class MemberExtensions
    {
        public static int GetHighestRolePosition(this IMember member)
        {
            return member.GetRoles().OrderBy(x => x.Value.Position).Last().Value.Position;
        }

        public static bool CanBeManaged(this IMember member)
        {
            var guild = member.GetGuild();
            var bot = guild.GetCurrentMember();

            return guild.OwnerId != member.Id &&
                   member.GetHighestRolePosition() < bot.GetHighestRolePosition();
        }
    }
}
