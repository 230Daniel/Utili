using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.Rest;
using static Utili.MessageSender;

namespace Utili.Features
{
    internal static class MessageFilter
    {
        public static async Task MessageReceived(SocketCommandContext context)
        {
            if (BotPermissions.IsMissingPermissions(context.Channel, new[] {ChannelPermission.ManageMessages}, out _))
            {
                return;
            }

            if (context.User.Id == Program._client.CurrentUser.Id && context.Message.Embeds.Count > 0)
            {
                Embed embed = context.Message.Embeds.First();
                if (embed.Author.HasValue)
                {
                    if (embed.Author.Value.Name == "Message deleted")
                    {
                        return;
                    }
                }
            }

            List<MessageFilterRow> rows = await Database.Data.MessageFilter.GetRowsAsync(context.Guild.Id, context.Channel.Id);

            if (rows.Count == 0)
            {
                return;
            }

            MessageFilterRow row = rows.First();

            if (!DoesMessageObeyRule(context, row, out string allowedTypes))
            {
                await context.Message.DeleteAsync();

                if (!context.User.IsBot)
                {
                    string deletionReason = $"Only messages {allowedTypes} are allowed in <#{context.Channel.Id}>";

                    RestUserMessage sentMessage = await SendFailureAsync(context.Channel, "Message deleted", deletionReason, supportLink: false);

                    await Task.Delay(5000);

                    await sentMessage.DeleteAsync();
                }
            }
        }

        private static bool DoesMessageObeyRule(SocketCommandContext context, MessageFilterRow row, out string allowedTypes)
        {
            switch (row.Mode)
            {
                case 0: // All
                    allowedTypes = "with anything";
                    return true;

                case 1: // Images
                    allowedTypes = "with images";
                    return IsImage(context);

                case 2: // Videos
                    allowedTypes = "with videos";
                    return IsVideo(context);

                case 3: // Media
                    allowedTypes = "with images or videos";
                    return IsImage(context) || IsVideo(context);

                case 4: // Music
                    allowedTypes = "with music";
                    return IsMusic(context) || IsVideo(context);

                case 5: // Attachments
                    allowedTypes = "with attachments";
                    return IsAttachment(context);

                case 6: // URLs
                    allowedTypes = "with valid urls";
                    return IsUrl(context);

                case 7: // URLs or Media
                    allowedTypes = "with images, videos or valid urls";
                    return IsImage(context) || IsVideo(context) || IsUrl(context);

                case 8: // RegEx
                    allowedTypes = $"which match the expresion `{row.Complex.Value}`";
                    return IsRegex(context, row.Complex.Value);

                default:
                    allowedTypes = "";
                    return true;
            }
        }

        public static bool IsImage(SocketCommandContext context)
        {
            List<string> validAttachmentExtensions = new List<string>
            {
                "png",
                "jpg"
            };

            List<string> filenames = context.Message.Attachments.Select(x => x.Filename).ToList();
            filenames.AddRange(context.Message.Content.Split(' ', '\n').Where(x => IsUrl(x) && x.Split("/").Last().Contains(".")));
            if(filenames.Any(x => validAttachmentExtensions.Contains(x.Split(".").Last().ToLower())))
                return true;

            if(context.Message.Embeds.Any(x => x.Image.HasValue)) 
                return true;

            return false;
        }

        public static bool IsVideo(SocketCommandContext context)
        {
            List<string> validAttachmentExtensions = new List<string>
            {
                "mp4",
                "mov",
                "wmv",
                "gif"
            };

            List<string> filenames = context.Message.Attachments.Select(x => x.Filename).ToList();
            filenames.AddRange(context.Message.Content.Split(' ', '\n').Where(x => IsUrl(x) && x.Split("/").Last().Contains(".")));
            if(filenames.Any(x => validAttachmentExtensions.Contains(x.Split(".").Last().ToLower())))
                return true;

            Regex youtubeRegex = new Regex(@"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$");
            foreach (string word in context.Message.Content.Split(' ', '\n'))
                if(youtubeRegex.IsMatch(word)) return true;

            return false;
        }

        public static bool IsMusic(SocketCommandContext context)
        {
            List<string> validAttachmentExtensions = new List<string>
            {
                "mp3",
                "m4a",
                "wav",
                "flac"
            };

            List<string> filenames = context.Message.Attachments.Select(x => x.Filename).ToList();
            filenames.AddRange(context.Message.Content.Split(' ', '\n').Where(x => IsUrl(x) && x.Split("/").Last().Contains(".")));
            if(filenames.Any(x => validAttachmentExtensions.Contains(x.Split(".").Last().ToLower())))
                return true;

            foreach (string word in context.Message.Content.Split(' ', '\n'))
                if (word.ToLower().Contains("spotify.com/") || word.ToLower().Contains("soundcloud.com/"))
                    return true;

            return false;
        }

        public static bool IsAttachment(SocketCommandContext context)
        {
            return context.Message.Attachments.Count > 0;
        }

        public static bool IsUrl(SocketCommandContext context)
        {
            foreach (string word in context.Message.Content.Split(' ', '\n'))
                if(IsUrl(word)) return true;

            return false;
        }

        private static bool IsUrl(string word)
        {
            return Uri.TryCreate(word, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsRegex(SocketCommandContext context, string pattern)
        {
            try
            {
                Regex regex = new Regex(pattern);
                return regex.IsMatch(context.Message.Content);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsEmbed(SocketCommandContext context)
        {
            return context.Message.Embeds.Count > 0;
        }
    }
}
