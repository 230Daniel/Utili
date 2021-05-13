using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Disqord;

namespace Utili.Extensions
{
    public static class MessageExtensions
    {
        public static ITextChannel GetChannel(this IMessage message, ulong guildId)
        {
            return (message.Client as DiscordClientBase).GetTextChannel(guildId, message.ChannelId);
        }

        public static string GetJumpUrl(this IMessage message, ulong guildId)
        {
            return $"https://discord.com/channels/{guildId}/{message.ChannelId}/{message.Id}";
        }

        public static bool IsImage(this IUserMessage message)
        {
            List<string> validAttachmentExtensions = new()
            {
                "png",
                "jpg"
            };

            List<string> filenames = message.Attachments.Select(x => x.Filename).ToList();
            filenames.AddRange(message.Content.Split(' ', '\n').Where(x => IsUrl(x) && x.Split("/").Last().Contains(".")));
            return filenames.Any(x => validAttachmentExtensions.Contains(x.Split(".").Last().ToLower())) || 
                   message.Embeds.Any(x => x.Image is not null);
        }

        public static bool IsVideo(this IUserMessage message)
        {
            List<string> validAttachmentExtensions = new()
            {
                "mp4",
                "mov",
                "wmv",
                "gif"
            };

            List<string> filenames = message.Attachments.Select(x => x.Filename).ToList();
            filenames.AddRange(message.Content.Split(' ', '\n').Where(x => IsUrl(x) && x.Split("/").Last().Contains(".")));
            if (filenames.Any(x => validAttachmentExtensions.Contains(x.Split(".").Last().ToLower())))
                return true;

            Regex youtubeRegex = new(@"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$");
            foreach (string word in message.Content.Split(' ', '\n'))
                if (youtubeRegex.IsMatch(word)) return true;

            return false;
        }

        public static bool IsMusic(this IUserMessage message)
        {
            List<string> validAttachmentExtensions = new()
            {
                "mp3",
                "m4a",
                "wav",
                "flac"
            };

            List<string> filenames = message.Attachments.Select(x => x.Filename).ToList();
            filenames.AddRange(message.Content.Split(' ', '\n').Where(x => IsUrl(x) && x.Split("/").Last().Contains(".")));
            if (filenames.Any(x => validAttachmentExtensions.Contains(x.Split(".").Last().ToLower())))
                return true;

            foreach (string word in message.Content.Split(' ', '\n'))
                if (word.ToLower().Contains("spotify.com/") || word.ToLower().Contains("soundcloud.com/"))
                    return true;

            return false;
        }

        public static bool IsAttachment(this IUserMessage message)
        {
            return message.Attachments.Count > 0;
        }

        public static bool IsUrl(this IUserMessage message)
        {
            foreach (string word in message.Content.Split(' ', '\n'))
                if (IsUrl(word)) return true;

            return false;
        }

        private static bool IsUrl(string word)
        {
            return Uri.TryCreate(word, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsRegex(this IUserMessage message, string pattern)
        {
            try
            {
                Regex regex = new(pattern);
                return regex.IsMatch(message.Content);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsEmbed(this IUserMessage message)
        {
            return message.Embeds.Count > 0;
        }
    }
}
