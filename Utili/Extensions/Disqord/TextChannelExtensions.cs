using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using Utili.Utils;

namespace Utili.Extensions
{
    public static class TextChannelExtensions
    {
        public static async Task<IUserMessage> SendInfoAsync(this ITextChannel channel, string title, string content = null)
        {
            LocalMessage message = new LocalMessageBuilder()
                .WithEmbed(MessageUtils.CreateEmbed(EmbedType.Info, title, content))
                .Build();

            return await channel.SendMessageAsync(message);
        }

        public static async Task<IUserMessage> SendSuccessAsync(this ITextChannel channel, string title, string content = null)
        {
            LocalMessage message = new LocalMessageBuilder()
                .WithEmbed(MessageUtils.CreateEmbed(EmbedType.Success, title, content))
                .Build();

            return await channel.SendMessageAsync(message);
        }

        public static async Task<IUserMessage> SendFailureAsync(this ITextChannel channel, string title, string content = null, bool supportLink = true)
        {
            LocalMessage message = new LocalMessageBuilder()
                .WithEmbed(MessageUtils.CreateEmbed(EmbedType.Failure, title, content))
                .Build();

            return await channel.SendMessageAsync(message);
        }

        public static async Task<IUserMessage> SendEmbedAsync(this ITextChannel channel, LocalEmbedBuilder embed)
        {
            LocalMessage message = new LocalMessageBuilder()
                .WithEmbed(embed)
                .Build();

            return await channel.SendMessageAsync(message);
        }
    }
}
