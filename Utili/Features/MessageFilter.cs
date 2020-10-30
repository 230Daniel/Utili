using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PostSharp.Aspects.Advices;

namespace Utili.Features
{
    internal class MessageFilter
    {
        public async Task MessageReceived(SocketCommandContext context)
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

            List<MessageFilterRow> rows = Database.Data.MessageFilter.GetRows(context.Guild.Id, context.Channel.Id);

            if (rows.Count == 0)
            {
                return;
            }

            MessageFilterRow row = rows.First();

            if (!DoesMessageObeyRule(context, row, out string allowedTypes))
            {
                await context.Message.DeleteAsync();

                if (BotPermissions.IsMissingPermissions(context.Channel, new[] {ChannelPermission.SendMessages}, out _))
                {
                    return;
                }

                string deletionReason = $"Only messages {allowedTypes} are allowed in <#{context.Channel.Id}>";

                RestUserMessage sentMessage = await MessageSender.SendFailureAsync(context.Channel, "Message deleted", deletionReason);

                await Task.Delay(5000);

                await sentMessage.DeleteAsync();
            }

        }

        public bool DoesMessageObeyRule(SocketCommandContext context, MessageFilterRow row, out string allowedTypes)
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
                    return true;

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
                    allowedTypes = "with urls";
                    return IsUrl(context);

                case 7: // URLs or Media
                    allowedTypes = "with images, videos or urls";
                    return IsImage(context) || IsVideo(context) || IsUrl(context);

                case 8: // RegEx
                    allowedTypes = $"which match regex \"{row.Complex}\"";
                    return IsRegex(context, row.Complex);

                default:
                    allowedTypes = "";
                    return true;
            }
        }

        public bool IsImage(SocketCommandContext context)
        {
            List<string> validAttachmentExtensions = new List<string>
            {
                "png",
                "jpg"
            };

            if(context.Message.Attachments.Count(x => validAttachmentExtensions.Contains(x.Filename.Split(".").Last().ToLower())) > 0)
            {
                return true;
            }

            foreach (string word in context.Message.Content.Split(" "))
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

        public bool IsVideo(SocketCommandContext context)
        {
            List<string> validAttachmentExtensions = new List<string>
            {
                "mp4",
                "mov",
                "wmv",
                "gif"
            };

            if(context.Message.Attachments.Count(x => validAttachmentExtensions.Contains(x.Filename.Split(".").Last().ToLower())) > 0)
            {
                return true;
            }

            Regex youtubeRegex = new Regex(@"^(http(s)??\:\/\/)?(www\.)?((youtube\.com\/watch\?v=)|(youtu.be\/))([a-zA-Z0-9\-_])+$");
            foreach (string word in context.Message.Content.Split(" "))
            {
                if (youtubeRegex.IsMatch(word))
                {
                    try
                    {
                        WebRequest request = WebRequest.Create(word);
                        request.Timeout = 2000;
                        request.GetResponse();

                        return true;
                    }
                    catch { }
                }
            }

            return false;
        }

        public bool IsMusic(SocketCommandContext context)
        {
            List<string> validAttachmentExtensions = new List<string>
            {
                "mp3",
                "m4a",
                "wav",
                "flac"
            };

            if(context.Message.Attachments.Count(x => validAttachmentExtensions.Contains(x.Filename.Split(".").Last().ToLower())) > 0)
            {
                return true;
            }

            foreach (string word in context.Message.Content.Split(" "))
            {
                if (word.ToLower().Contains("spotify.com/") || word.ToLower().Contains("soundcloud.com/"))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAttachment(SocketCommandContext context)
        {
            return context.Message.Attachments.Count > 0;
        }

        public bool IsUrl(SocketCommandContext context)
        {
            foreach (string word in context.Message.Content.Split(" "))
            {
                try
                {
                    WebRequest request = WebRequest.Create(word);
                    request.Timeout = 2000;
                    request.GetResponse();

                    return true;
                }
                catch { }
            }

            return false;
        }

        public bool IsRegex(SocketCommandContext context, string pattern)
        {
            try
            {
                Regex regex = new Regex(pattern);

                foreach (string word in context.Message.Content.Split(" "))
                {
                    if (regex.IsMatch(word))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
