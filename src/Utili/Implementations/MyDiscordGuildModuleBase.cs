﻿using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Utili.Implementations.Views;
using Utili.Utils;

namespace Utili.Implementations
{
    public class MyDiscordGuildModuleBase : DiscordGuildModuleBase
    {
        protected DiscordCommandResult Info(string title, string content = null)
            => Response(MessageUtils.CreateEmbed(EmbedType.Info, title, content));

        protected DiscordCommandResult Success(string title, string content = null)
            => Response(MessageUtils.CreateEmbed(EmbedType.Success, title, content));

        protected DiscordCommandResult Failure(string title, string content = null)
            => Response(MessageUtils.CreateEmbed(EmbedType.Failure, title, content));

        protected async Task<bool> ConfirmAsync(ConfirmViewOptions options)
        {
            await using var yield = Context.BeginYield();
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
}