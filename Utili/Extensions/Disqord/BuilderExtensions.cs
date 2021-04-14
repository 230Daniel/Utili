using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
