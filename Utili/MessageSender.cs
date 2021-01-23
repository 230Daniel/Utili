using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Utili
{
    internal static class MessageSender
    {
        public static async Task<RestUserMessage> SendSuccessAsync(IChannel channel, string title, string message = null, string footer = null)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;
            if(BotPermissions.IsMissingPermissions(channel, new [] {ChannelPermission.SendMessages}, out _)) return null;

            return await textChannel.SendMessageAsync(embed: GenerateEmbed(EmbedType.Success, title, message, footer));
        }

        public static async Task<RestUserMessage> SendFailureAsync(IChannel channel, string title, string message = null, string footer = null, bool supportLink = true)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;
            if(BotPermissions.IsMissingPermissions(channel, new [] {ChannelPermission.SendMessages}, out _)) return null;

            if (!string.IsNullOrEmpty(message) && supportLink)
            {
                message += "\n[Support Server](https://discord.gg/WsxqABZ)";
            }

            return await textChannel.SendMessageAsync(embed: GenerateEmbed(EmbedType.Failure, title, message, footer));
        }

        public static async Task<RestUserMessage> SendInfoAsync(IChannel channel, string title, string message, string footer = null, (string, string)[] fields = null, Color? colour = null)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;
            if(BotPermissions.IsMissingPermissions(channel, new [] {ChannelPermission.SendMessages}, out _)) return null;

            return await textChannel.SendMessageAsync(embed: GenerateEmbed(EmbedType.Info, title, message, footer, fields, colour));
        }

        public static async Task<RestUserMessage> SendEmbedAsync(IChannel channel, Embed embed, string text = null)
        {
            ISocketMessageChannel textChannel = channel as ISocketMessageChannel;
            if(BotPermissions.IsMissingPermissions(channel, new [] {ChannelPermission.SendMessages}, out _)) return null;

            return await textChannel.SendMessageAsync(text, embed: embed);
        }

        public static Embed GenerateEmbed(EmbedType embedType, string title, string content = null, string footer = null, (string, string)[] fields = null, Color? colour = null)
        {
            EmbedBuilder embed = new EmbedBuilder();

            EmbedAuthorBuilder author = new EmbedAuthorBuilder();

            if (!string.IsNullOrEmpty(title))
            {
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
            }

            if (colour.HasValue) embed.WithColor(colour.Value);

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

        public enum EmbedType
        {
            Info,
            Success,
            Failure
        }
    }
}
