using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

            if(context.Message.Attachments.Any(x => validAttachmentExtensions.Contains(x.Filename.Split(".").Last().ToLower())))
            {
                return true;
            }

            if(context.Message.Embeds.Any(x => x.Image.HasValue)) return true;

            foreach (string word in context.Message.Content.Split(' ', '\n'))
            {
                try
                {
                    WebRequest request = WebRequest.Create(word);
                    request.Timeout = 2000;
                    WebResponse response = request.GetResponse();

                    if (response.ContentType.ToLower().StartsWith("image/"))
                    {
                        return true;
                    }
                }
                catch { }
            }
            
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

            if(context.Message.Attachments.Any(x => validAttachmentExtensions.Contains(x.Filename.Split(".").Last().ToLower())))
            {
                return true;
            }

            Regex youtubeRegex = new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");
            foreach (string word in context.Message.Content.Split(' ', '\n'))
            {
                Match youtubeMatch = youtubeRegex.Match(word);
                if (youtubeMatch.Success)
                {
                    string id = youtubeMatch.Groups[1].Value;

                    try
                    {
                        HttpWebRequest request =
                            (HttpWebRequest) WebRequest.Create($"https://img.youtube.com/vi/{id}/mqdefault.jpg");
                        HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                        Stream stream = response.GetResponseStream();
                        System.Drawing.Image thumbnail = System.Drawing.Image.FromStream(stream);
                        stream.Close();

                        // If the video doesn't exist then the thumbnail is a default 120x90 image.
                        if(thumbnail.Width != 120 && thumbnail.Width != 90) return true;
                    }
                    catch { }
                }
            }

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

            if(context.Message.Attachments.Any(x => validAttachmentExtensions.Contains(x.Filename.Split(".").Last().ToLower())))
            {
                return true;
            }

            foreach (string word in context.Message.Content.Split(' ', '\n'))
            {
                if (word.ToLower().Contains("spotify.com/") || word.ToLower().Contains("soundcloud.com/"))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsAttachment(SocketCommandContext context)
        {
            return context.Message.Attachments.Count > 0;
        }

        public static bool IsUrl(SocketCommandContext context)
        {
            foreach (string word in context.Message.Content.Split(' ', '\n'))
            {
                try
                {
                    if (Uri.TryCreate(word, UriKind.Absolute, out Uri uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                    {
                        using(MyClient client = new MyClient()) {
                            client.HeadOnly = true;
                            client.DownloadString(word);
                        }
                        return true;
                    }
                }
                catch { }
            }

            return false;
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
    }

    // https://stackoverflow.com/a/924682/11089240
    internal class MyClient : WebClient
    {
        public bool HeadOnly { get; set; }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest req = base.GetWebRequest(address);
            if (HeadOnly && req.Method == "GET")
            {
                req.Method = "HEAD";
            }
            return req;
        }
    }
}
