using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Qmmands;

namespace Utili.Implementations
{
    public class RequireBotParameterChannelPermissionsAttribute : DiscordGuildParameterCheckAttribute
    {
        public Permission Permissions { get; }

        public RequireBotParameterChannelPermissionsAttribute(Permission permissions)
        {
            Permissions = permissions;
        }

        public override bool CheckType(Type type)
            => typeof(IGuildChannel).IsAssignableFrom(type);

        public override ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            IGuildChannel channel = (IGuildChannel) argument;
            IReadOnlyDictionary<Snowflake, CachedRole> roles = context.Guild.GetMember(context.Bot.CurrentUser.Id).GetRoles();
            ChannelPermissions permissions = Disqord.Discord.Permissions.CalculatePermissions(context.Guild, channel, context.Guild.GetMember(context.Bot.CurrentUser.Id), roles.Values);

            return permissions.Has(Permissions) ? 
                Success() : 
                Failure($"You lack the necessary channel permissions in {channel} ({Permissions - permissions}) to execute this.");
        }
    }
}
