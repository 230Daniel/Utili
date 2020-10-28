using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database.Data;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Utili.Features
{
    internal class MessageFilter
    {
        public static async Task MessageReceived(SocketCommandContext context)
        {
            List<MessageFilterRow> rows = Database.Data.MessageFilter.GetRows(context.Guild.Id, context.Channel.Id);

            if (rows.Count == 0)
            {
                return;
            }

            MessageFilterRow row = rows.First();

            if (!DoesMessageObeyRule(context, row, out string allowedTypes))
            {
                string deletionReason = $"Only messages {allowedTypes} are allowed in <#{context.Channel.Id}>";
            }

        }

        public static bool DoesMessageObeyRule(SocketCommandContext context, MessageFilterRow row, out string allowedTypes)
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

                case 7: // RegEx
                    allowedTypes = $"following regex {row.Complex}";
                    return IsRegex(context, row.Complex);

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

        public static bool IsVideo(SocketCommandContext context)
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

        public static bool IsMusic(SocketCommandContext context)
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

        public static bool IsAttachment(SocketCommandContext context)
        {
            return context.Message.Attachments.Count > 0;
        }

        public static bool IsUrl(SocketCommandContext context)
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

        public static bool IsRegex(SocketCommandContext context, string regexCode)
        {
            Regex regex = new Regex(regexCode);

            foreach (string word in context.Message.Content.Split(" "))
            {
                if (regex.IsMatch(word))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
