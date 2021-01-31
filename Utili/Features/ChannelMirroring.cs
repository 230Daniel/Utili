using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;

namespace Utili.Features
{
    internal static class ChannelMirroring
    {
        public static async Task MessageReceived(SocketCommandContext context)
        {
            if(context.User.IsWebhook) return;

            List<ChannelMirroringRow> rows = await Database.Data.ChannelMirroring.GetRowsAsync(context.Guild.Id, context.Channel.Id);
            if(rows.Count == 0) return;
            ChannelMirroringRow row = rows.First();

            SocketTextChannel channel = context.Guild.GetTextChannel(row.ToChannelId);
            if(channel is null) return;

            if (BotPermissions.IsMissingPermissions(channel, new[] {ChannelPermission.ManageWebhooks}, out _)) return;

            RestWebhook webhook = null;
            try { webhook = await GetWebhookAsync(channel, row.WebhookId); } catch { }

            if (webhook is null)
            {
                FileStream avatar = File.OpenRead("Avatar.png");
                webhook = await channel.CreateWebhookAsync("Utili Mirroring", avatar);
                avatar.Close();

                row.WebhookId = webhook.Id;
                await Database.Data.ChannelMirroring.SaveWebhookIdAsync(row);
            }

            string username = $"{context.User} in #{context.Channel}";
            string avatarUrl = context.User.GetAvatarUrl();
            if (string.IsNullOrEmpty(avatarUrl)) avatarUrl = context.User.GetDefaultAvatarUrl();

            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.None);
            DiscordWebhookClient webhookClient = new DiscordWebhookClient(webhook);
            if (!(string.IsNullOrEmpty(context.Message.Content) && context.Message.Embeds.Count == 0))
            {
                await webhookClient.SendMessageAsync(context.Message.Content, false, context.Message.Embeds, username, avatarUrl, allowedMentions: allowedMentions);
            }

            foreach (Attachment attachment in context.Message.Attachments)
            {
                WebRequest request = WebRequest.Create(attachment.Url);
                Stream stream = (await request.GetResponseAsync()).GetResponseStream();
                await webhookClient.SendFileAsync(stream, attachment.Filename, "", username: username, avatarUrl: avatarUrl);
                stream.Close();
            }
        }

        private static List<(ulong, ulong, RestWebhook)> _cachedWebhooks = new List<(ulong, ulong, RestWebhook)>();
        private static async Task<RestWebhook> GetWebhookAsync(SocketTextChannel channel, ulong webhookId)
        {
            if(_cachedWebhooks.Any(x => x.Item1 == channel.Id && x.Item2 == webhookId))
            {
                return _cachedWebhooks.First(x => x.Item1 == channel.Id && x.Item2 == webhookId).Item3;
            }
            else
            {
                _cachedWebhooks.Add((channel.Id, webhookId, await channel.GetWebhookAsync(webhookId)));
                return _cachedWebhooks.First(x => x.Item1 == channel.Id && x.Item2 == webhookId).Item3;
            }
        }
    }
}
