using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Disqord;

namespace Utili.Extensions
{
    static class BuilderExtensions
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

        public static LocalEmbedBuilder WithOptionalAuthor(this LocalEmbedBuilder builder, string name, string iconUrl)
        {
            builder.Author = new LocalEmbedAuthorBuilder()
                .WithOptionalName(name)
                .WithIconUrl(iconUrl);
            return builder;
        }

        public static LocalEmbedAuthorBuilder WithOptionalName(this LocalEmbedAuthorBuilder builder, string name)
        {
            if (!string.IsNullOrWhiteSpace(name)) builder.WithName(name);
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
