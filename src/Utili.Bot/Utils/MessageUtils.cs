using Disqord;
using Utili.Bot.Extensions;

namespace Utili.Bot.Utils;

public static class MessageUtils
{
    public static LocalEmbed CreateEmbed(EmbedType type, string title, string content = null)
    {
        var embed = new LocalEmbed();

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