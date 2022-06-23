﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Database.Entities;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Database.Extensions;
using Disqord.Http;
using Utili.Extensions;

namespace Utili.Services
{
    public class ChannelMirroringService
    {
        private readonly ILogger<ChannelMirroringService> _logger;
        private readonly DiscordClientBase _client;
        private readonly WebhookService _webhookService;

        public ChannelMirroringService(ILogger<ChannelMirroringService> logger, DiscordClientBase client, WebhookService webhookService)
        {
            _logger = logger;
            _client = client;
            _webhookService = webhookService;
        }

        public async Task MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
        {
            try
            {
                if (e.Message is not IUserMessage { WebhookId: null } userMessage) return;

                var db = scope.GetDbContext();
                var config = await db.ChannelMirroringConfigurations.GetForGuildChannelAsync(e.GuildId.Value, e.ChannelId);
                if (config is null) return;

                var guild = _client.GetGuild(e.GuildId.Value);
                var destinationChannel = guild.GetTextChannel(config.DestinationChannelId);
                if (destinationChannel is null) return;

                if (!destinationChannel.BotHasPermissions(Permission.ViewChannels | Permission.ManageWebhooks)) return;

                string username;
                string avatarUrl;
                string content;

                if (config.AuthorDisplayMode == ChannelMirroringAuthorDisplayMode.WebhookName)
                {
                    username = $"{e.Message.Author} in #{e.Channel.Name}";
                    avatarUrl = e.Message.Author.GetAvatarUrl();
                    content = e.Message.Content;
                }
                else
                {
                    var bot = _client.GetGuild(e.GuildId.Value).GetCurrentMember();
                    username = bot.Nick ?? bot.Name;
                    avatarUrl = null;
                    content = e.Message.Content.Contains('\n')
                        ? $"{e.Message.Author.Mention} in {e.Channel.Mention}:\n{e.Message.Content}"
                        : $"{e.Message.Author.Mention} in {e.Channel.Mention}: {e.Message.Content}";
                }

                using var httpClient = new HttpClient();

                var attachmentChunks = new List<LocalAttachment[]>();
                var currentAttachmentChunk = new List<LocalAttachment>();
                var currentAttachmentChunkSize = 0;

                foreach (var attachment in userMessage.Attachments.Where(x => x.FileSize < 8000000))
                {
                    var stream = await httpClient.GetStreamAsync(attachment.ProxyUrl);
                    if (currentAttachmentChunkSize + attachment.FileSize >= 8000000)
                    {
                        attachmentChunks.Add(currentAttachmentChunk.ToArray());
                        currentAttachmentChunk = new();
                        currentAttachmentChunkSize = 0;
                    }

                    currentAttachmentChunk.Add(new LocalAttachment(stream, attachment.FileName));
                    currentAttachmentChunkSize += attachment.FileSize;
                }

                if (currentAttachmentChunk.Any())
                    attachmentChunks.Add(currentAttachmentChunk.ToArray());

                var message = new LocalWebhookMessage()
                    .WithName(username)
                    .WithAvatarUrl(avatarUrl)
                    .WithOptionalContent(content)
                    .WithEmbeds(userMessage.Embeds.Where(x => x.IsRich()).Select(LocalEmbed.FromEmbed))
                    .WithAllowedMentions(LocalAllowedMentions.None);

                if (attachmentChunks.Any())
                    message.WithAttachments(attachmentChunks[0]);

                try
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var webhook = await _webhookService.GetWebhookAsync(destinationChannel.Id);

                        try
                        {
                            await _client.ExecuteWebhookAsync(webhook.Id, webhook.Token, message);

                            foreach (var attachmentChunk in attachmentChunks.Skip(1))
                            {
                                message = new LocalWebhookMessage()
                                    .WithName(username)
                                    .WithAvatarUrl(avatarUrl)
                                    .WithAllowedMentions(LocalAllowedMentions.None)
                                    .WithAttachments(attachmentChunk);

                                await _client.ExecuteWebhookAsync(webhook.Id, webhook.Token, message);

                                if (userMessage.Attachments.Any(x => x.FileSize >= 8000000))
                                {
                                    message = new LocalWebhookMessage()
                                        .WithName(username)
                                        .WithAvatarUrl(avatarUrl)
                                        .WithContent(string.Concat(userMessage.Attachments.Where(x => x.FileSize >= 8000000).Select(x => x.ProxyUrl + "\n")))
                                        .WithAllowedMentions(LocalAllowedMentions.None);

                                    await _client.ExecuteWebhookAsync(webhook.Id, webhook.Token, message);
                                }
                            }

                            break;
                        }
                        catch (RestApiException ex) when (ex.StatusCode == HttpResponseStatusCode.NotFound)
                        {
                            await _webhookService.ReportInvalidWebhookAsync(destinationChannel.Id, webhook.Id);
                            if (i == 1) throw;
                        }
                    }
                }
                finally
                {
                    foreach (var attachmentChunk in attachmentChunks)
                    foreach (var attachment in attachmentChunk)
                        attachment.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
            }
        }
    }
}