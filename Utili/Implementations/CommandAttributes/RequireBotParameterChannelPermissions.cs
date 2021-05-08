using System;
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
            if (argument is null) return Success();
            
            IGuildChannel channel = (IGuildChannel) argument;
            ChannelPermissions permissions = context.CurrentMember.GetChannelPermissions(channel);

            return permissions.Has(Permissions) ? 
                Success() : 
                Failure($"The bot lacks the necessary channel permissions in {channel} ({Permissions - permissions}) to execute this.");
        }
    }
}
