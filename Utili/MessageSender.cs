using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Utili
{
    internal class MessageSender
    {
        public static async Task SendSuccessAsync(IChannel channel, string title, string message)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;
            if(textChannel == null) return;

            await textChannel.SendMessageAsync($"{title}: {message}");
        }

        public static async Task SendFailureAsync(IChannel channel, string title, string message)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;
            if(textChannel == null) return;

            await textChannel.SendMessageAsync($"{title}: {message}");
        }
    }
}
