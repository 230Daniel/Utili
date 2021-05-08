using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Qmmands;

namespace Utili.Implementations
{
    public class RequireAuthorParameterChannelPermissionsAttribute : DiscordGuildParameterCheckAttribute
    {
        public Permission Permissions { get; }

        public RequireAuthorParameterChannelPermissionsAttribute(Permission permissions)
        {
            Permissions = permissions;
        }

        public override bool CheckType(Type type)
            => typeof(IGuildChannel).IsAssignableFrom(type);

        public override ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            IGuildChannel channel = (IGuildChannel) argument;
            ChannelPermissions permissions = context.Author.GetChannelPermissions(channel);

            return permissions.Has(Permissions) ? 
                Success() : 
                Failure($"You lack the necessary channel permissions in {channel} ({Permissions - permissions}) to execute this.");
        }
    }
}
