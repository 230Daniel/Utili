using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Qmmands;
using Utili.Extensions;

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
            ChannelPermissions permissions = context.CurrentMember.GetChannelPermissions(channel);

            return permissions.Has(Permissions) ? 
                Success() : 
                Failure($"I lack the necessary channel permissions in {channel} ({Permissions - permissions}) to execute this.");
        }
    }
}
