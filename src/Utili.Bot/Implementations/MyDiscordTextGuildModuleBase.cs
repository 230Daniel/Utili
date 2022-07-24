using System;
using System.Threading.Tasks;
using Disqord.Bot.Commands.Text;
using Qmmands;
using Utili.Bot.Implementations.Views;
using Utili.Bot.Utils;

namespace Utili.Bot.Implementations;

public class MyDiscordTextGuildModuleBase : DiscordTextGuildModuleBase
{
    protected IResult Info(string title, string content = null)
        => Response(MessageUtils.CreateEmbed(EmbedType.Info, title, content));

    protected IResult Success(string title, string content = null)
        => Response(MessageUtils.CreateEmbed(EmbedType.Success, title, content));

    protected IResult Failure(string title, string content = null)
        => Response(MessageUtils.CreateEmbed(EmbedType.Failure, title, content));

    protected async Task<bool> ConfirmAsync(ConfirmViewOptions options)
    {
        var view = new ConfirmView(Context.Author.Id, options);

        try
        {
            await View(view, TimeSpan.FromSeconds(30));
        }
        catch (TaskCanceledException)
        {
            return false;
        }

        return view.Result;
    }
}
