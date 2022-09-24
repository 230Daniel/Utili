using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Qmmands;
using Utili.Bot.Extensions;

namespace Utili.Bot.Commands;

public class RequireNotThreadAttribute : DiscordGuildCheckAttribute
{
    public override ValueTask<IResult> CheckAsync(IDiscordGuildCommandContext context)
    {
        return context.GetChannel() is IThreadChannel ?
            Results.Failure("This command can not be used in a thread channel.") :
            Results.Success;
    }
}
