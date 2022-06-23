using Disqord;

namespace Utili.Extensions
{
    public static class BuilderExtensions
    {
        public static LocalMessage WithOptionalContent(this LocalMessage builder, string content)
        {
            builder.Content = string.IsNullOrWhiteSpace(content) ? null : content;
            return builder;
        }

        public static LocalMessage WithRequiredContent(this LocalMessage builder, string content)
        {
            builder.Content = string.IsNullOrWhiteSpace(content) ? "\u200b" : content;
            return builder;
        }

        public static LocalWebhookMessage WithOptionalContent(this LocalWebhookMessage builder, string content)
        {
            builder.Content = string.IsNullOrWhiteSpace(content) ? null : content;
            return builder;
        }

        public static LocalEmbed WithOptionalAuthor(this LocalEmbed builder, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.Author = new LocalEmbedAuthor()
                    .WithName(name);
            }

            return builder;
        }

        public static LocalEmbed WithOptionalAuthor(this LocalEmbed builder, string name, string iconUrl)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.Author = new LocalEmbedAuthor()
                    .WithName(name)
                    .WithIconUrl(iconUrl);
            }

            return builder;
        }

        public static LocalEmbed WithOptionalFooter(this LocalEmbed builder, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.Footer = new LocalEmbedFooter()
                    .WithText(text);
            }

            return builder;
        }

        public static LocalEmbed AddInlineField(this LocalEmbed builder, string name, string value)
        {
            builder.AddField(name, value, true);
            return builder;
        }
    }
}
