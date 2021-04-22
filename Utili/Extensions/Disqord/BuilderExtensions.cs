using Disqord;

namespace Utili.Extensions
{
    public static class BuilderExtensions
    {
        public static LocalMessageBuilder WithOptionalContent(this LocalMessageBuilder builder, string content)
        {
            builder.Content = string.IsNullOrWhiteSpace(content) ? null : content;
            return builder;
        }

        public static LocalWebhookMessageBuilder WithOptionalContent(this LocalWebhookMessageBuilder builder, string content)
        {
            builder.Content = string.IsNullOrWhiteSpace(content) ? null : content;
            return builder;
        }

        public static LocalEmbedBuilder WithOptionalAuthor(this LocalEmbedBuilder builder, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.Author = new LocalEmbedAuthorBuilder()
                    .WithName(name);
            }
            
            return builder;
        }

        public static LocalEmbedBuilder WithOptionalAuthor(this LocalEmbedBuilder builder, string name, string iconUrl)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.Author = new LocalEmbedAuthorBuilder()
                    .WithName(name)
                    .WithIconUrl(iconUrl);
            }
            
            return builder;
        }

        public static LocalEmbedBuilder WithOptionalFooter(this LocalEmbedBuilder builder, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.Footer = new LocalEmbedFooterBuilder()
                    .WithText(text);
            }
            
            return builder;
        }
    }
}
