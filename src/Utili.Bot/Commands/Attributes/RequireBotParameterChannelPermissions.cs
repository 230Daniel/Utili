using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Gateway;
using Qmmands;
using Utili.Bot.Extensions;

namespace Utili.Bot.Commands;

public class RequireBotParameterChannelPermissionsAttribute : DiscordGuildParameterCheckAttribute
{
    public Permissions Permissions { get; }

    public RequireBotParameterChannelPermissionsAttribute(Permissions permissions)
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
        if (argument is null) return Results.Success;

        var channel = (IGuildChannel)argument;
        var permissions = context.GetCurrentMember().CalculateChannelPermissions(channel);

        return permissions.HasFlag(Permissions) ?
            Results.Success :
            Results.Failure($"The bot lacks the necessary channel permissions in {channel} ({Permissions & ~permissions}) to execute this.");
    }
}
