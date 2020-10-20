using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Org.BouncyCastle.Math.Field;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Utili
{
    internal class MessageSender
    {
        public static async Task<RestUserMessage> SendSuccessAsync(IChannel channel, string title, string message = null, string footer = null)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;

            return await textChannel.SendMessageAsync(embed: GenerateEmbed(EmbedType.Success, title, message, footer));
        }

        public static async Task<RestUserMessage> SendFailureAsync(IChannel channel, string title, string message = null, string footer = null)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;

            if (!string.IsNullOrEmpty(message))
            {
                message += "\n[Support Server](https://discord.gg/hCYWk9x)";
            }

            return await textChannel.SendMessageAsync(embed: GenerateEmbed(EmbedType.Failure, title, message, footer));
        }

        public static async Task<RestUserMessage> SendInfoAsync(IChannel channel, string title, string message, string footer = null, (string, string)[] fields = null)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;

            return await textChannel.SendMessageAsync(embed: GenerateEmbed(EmbedType.Info, title, message, footer, fields));
        }

        private static Embed GenerateEmbed(EmbedType embedType, string title, string content = null, string footer = null, (string, string)[] fields = null)
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

            if (fields != null)
            {
                foreach ((string, string) field in fields)
                {
                    embed.AddField(field.Item1, field.Item2, true);
                }
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
