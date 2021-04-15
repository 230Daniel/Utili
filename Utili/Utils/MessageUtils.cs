using Disqord;
using Utili.Extensions;

namespace Utili.Utils
{
    static class MessageUtils
    {
        public static LocalEmbedBuilder CreateEmbed(EmbedType type, string title, string content = null)
        {
            LocalEmbedBuilder embed = new LocalEmbedBuilder();

            switch (type)
            {
                case EmbedType.Info:
                    embed.WithOptionalAuthor(title);
                    embed.WithColor(new Color(67, 181, 129));
                    break;

                case EmbedType.Success:
                    embed.WithOptionalAuthor(title, "https://i.imgur.com/XnVa7ta.png");
                    embed.WithColor(new Color(67, 181, 129));
                    break;

                case EmbedType.Failure:
                    embed.WithOptionalAuthor(title, "https://i.imgur.com/Sg4663k.png");
                    embed.WithColor(new Color(181, 67, 67));
                    break;
            }

            embed.WithDescription(content);

            return embed;
        }
    }

    public enum EmbedType
    {
        Info,
        Success,
        Failure
    }
}
