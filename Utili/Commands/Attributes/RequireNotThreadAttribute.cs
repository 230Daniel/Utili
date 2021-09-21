using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Utili.Commands
{
    public class RequireNotThreadAttribute : DiscordGuildCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
        {
            return context.Channel is IThreadChannel ? Failure("This command can not be used in a thread channel.") : Success();
        }
    }
}
