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

            await textChannel.SendMessageAsync(embed: GenerateEmbed(EmbedType.Success, title, message));
        }

        public static async Task SendFailureAsync(IChannel channel, string title, string message)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;
            if(textChannel == null) return;

            await textChannel.SendMessageAsync(embed: GenerateEmbed(EmbedType.Failure, title, message));
        }

        private static Embed GenerateEmbed(EmbedType embedType, string title, string content = null, string footer = null)
        {
            EmbedBuilder embed = new EmbedBuilder();

            EmbedAuthorBuilder author = new EmbedAuthorBuilder();

            switch (embedType)
            {
                case EmbedType.Info:
                    embed.WithColor(67, 181, 129);
                    break;

                case EmbedType.Success:
                    embed.WithColor(67, 181, 129);
                    author.WithIconUrl("https://i.imgur.com/XnVa7ta.png");
                    break;

                case EmbedType.Failure:
                    embed.WithColor(181, 67, 67);
                    author.WithIconUrl("https://i.imgur.com/Sg4663k.png");
                    break;
            }

            author.WithName(title);
            embed.WithAuthor(author);

            if (!string.IsNullOrEmpty(content))
            {
                embed.WithDescription(content);
            }

            if (!string.IsNullOrEmpty(footer))
            {
                embed.WithFooter(footer);
            }

            return embed.Build();
        }

        private enum EmbedType
        {
            Info,
            Success,
            Failure
        }
    }
}
