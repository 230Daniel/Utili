using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Gateway;
using Qmmands;

namespace Utili.Bot.Commands;

public class RequireAuthorParameterChannelPermissionsAttribute : DiscordGuildParameterCheckAttribute
{
    public Permissions Permissions { get; }

    public RequireAuthorParameterChannelPermissionsAttribute(Permissions permissions)
    {
        Permissions = permissions;
    }

    public override bool CanCheck(IParameter parameter, object value)
    {
        var parameterType = parameter.GetTypeInformation().ActualType;
        return typeof(IGuildChannel).IsAssignableFrom(parameterType);
    }

    public override ValueTask<IResult> CheckAsync(IDiscordGuildCommandContext context, IParameter parameter, object argument)
    {
        var channel = (IGuildChannel)argument;
        var permissions = context.Author.CalculateChannelPermissions(channel);

        return permissions.HasFlag(Permissions) ?
            Results.Success :
            Results.Failure($"You lack the necessary channel permissions in {channel} ({Permissions & ~permissions}) to execute this.");
    }
}
