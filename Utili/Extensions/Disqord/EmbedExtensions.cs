using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;

namespace Utili.Extensions
{
    static class EmbedExtensions
    {
        public static LocalEmbedBuilder ToLocalEmbedBuilder(this Embed embed)
        {
            return new LocalEmbedBuilder()
                .WithAuthor(embed.Author.Name, embed.Author.IconUrl, embed.Author.Url)
                .WithColor(embed.Color)
                .WithDescription(embed.Description)
                .WithFields(embed.Fields.Select(x => x.ToLocalEmbedFieldBuilder()))
                .WithFooter(embed.Footer.Text, embed.Footer.IconUrl)
                .WithImageUrl(embed.Image.Url)
                .WithThumbnailUrl(embed.Thumbnail.Url)
                .WithTimestamp(embed.Timestamp)
                .WithTitle(embed.Title)
                .WithUrl(embed.Url);
        }

        static LocalEmbedFieldBuilder ToLocalEmbedFieldBuilder(this EmbedField field)
        {
            return new LocalEmbedFieldBuilder()
                .WithIsInline(field.IsInline)
                .WithName(field.Name)
                .WithValue(field.Value);
        }
    }
}
