﻿using System.Threading.Tasks;
using Disqord.Bot;
using Utili.Implementations.Views;

namespace Utili.Implementations
{
    public class DiscordInteractiveGuildModuleBase : DiscordGuildModuleBase
    {
        protected async Task<bool> ConfirmAsync(string title, string content = null, string confirmButtonLabel = "Confirm")
        {
            var view = new ConfirmView(Context.Author.Id, title, content, confirmButtonLabel);
            await View(view);
            return view.Result;
        }
    }
}
