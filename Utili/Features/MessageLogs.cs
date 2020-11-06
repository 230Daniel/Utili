﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Features
{
    class MessageLogs
    {
        public async Task MessageReceived(SocketCommandContext context)
        {
            if (context.User.IsBot || context.Channel is SocketDMChannel) return;

            MessageLogsRow row = Database.Data.MessageLogs.GetRow(context.Guild.Id);

            if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(context.Channel.Id)) return;

            MessageLogsMessageRow message = new MessageLogsMessageRow()
            {
                GuildId = context.Guild.Id,
                ChannelId = context.Channel.Id,
                MessageId = context.Message.Id,
                UserId = context.User.Id,
                Timestamp = DateTime.UtcNow,
                Content = EString.FromDecoded(context.Message.Content)
            };

            Database.Data.MessageLogs.SaveMessage(message);
            Database.Data.MessageLogs.DeleteOldMessages(context.Guild.Id, context.Channel.Id,
                Premium.IsPremium(context.Guild.Id));
        }

        public async Task MessageEdited(SocketCommandContext context)
        {
            MessageLogsRow row = Database.Data.MessageLogs.GetRow(context.Guild.Id);
            if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(context.Channel.Id)) return;

            MessageLogsMessageRow message = Database.Data.MessageLogs.GetMessage(context.Guild.Id, context.Channel.Id, context.Message.Id);
            if (message == null) return;
            Embed embed = GetEditedEmbed(message, context);

            message.Content = EString.FromDecoded(context.Message.Content);
            Database.Data.MessageLogs.SaveMessage(message);

            SocketTextChannel channel = context.Guild.GetTextChannel(row.EditedChannelId);
            if(channel == null) return;
            await channel.SendMessageAsync(embed: embed);
        }

        public async Task MessageDeleted(SocketGuild guild, SocketTextChannel channel, ulong messageId)
        {
            MessageLogsRow row = Database.Data.MessageLogs.GetRow(guild.Id);
            if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(channel.Id)) return;

            MessageLogsMessageRow message = Database.Data.MessageLogs.GetMessage(guild.Id, channel.Id, messageId);
            if(message == null) return;
            Embed embed = GetDeletedEmbed(guild, channel, message);

            Database.Data.MessageLogs.DeleteMessagesById(new[] {message.Id});

            SocketTextChannel logChannel = guild.GetTextChannel(row.DeletedChannelId);
            if(logChannel == null) return;
            await logChannel.SendMessageAsync(embed: embed);
        }

        public async Task MessagesBulkDeleted(SocketGuild guild, SocketTextChannel channel, List<ulong> messageIds)
        {
            MessageLogsRow row = Database.Data.MessageLogs.GetRow(guild.Id);
            if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(channel.Id)) return;

            Embed embed = GetBulkDeletedEmbed(guild, channel, messageIds.Count);

            Database.Data.MessageLogs.DeleteMessagesByMessageId(guild.Id, channel.Id, messageIds.ToArray());

            SocketTextChannel logChannel = guild.GetTextChannel(row.DeletedChannelId);
            if(logChannel == null) return;
            await logChannel.SendMessageAsync(embed: embed);
        }

        private Embed GetEditedEmbed(MessageLogsMessageRow before, SocketCommandContext after)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(66, 182, 245);
            embed.WithDescription($"**Message by {after.User.Mention} edited in {(after.Channel as SocketTextChannel).Mention}** [Jump]({after.Message.GetJumpUrl()})");

            if(before.Content.Value.Length > 1024 || after.Message.Content.Length > 1024)
            {
                if(before.Content.Value.Length < 2024 - embed.Description.Length - 2)
                {
                    embed.Description += $"\n{before.Content}";
                }
                else
                {
                    embed.Description += "\nThe message is too large to fit in this embed";
                }

                embed.WithFooter($"Message {before.MessageId}");
                embed.WithTimestamp(new DateTimeOffset(before.Timestamp));
            }
            else
            {
                embed.AddField("Before", before.Content);
                embed.AddField("After", after.Message.Content);

                embed.WithFooter("Sent");
                embed.WithTimestamp(new DateTimeOffset(before.Timestamp));
            }

            string avatar = after.User.GetAvatarUrl();
            if (string.IsNullOrEmpty(avatar)) avatar = after.User.GetDefaultAvatarUrl();

            embed.WithAuthor(new EmbedAuthorBuilder
            {
                Name = $"{after.User.Username}#{after.User.Discriminator}",
                IconUrl = avatar
            });

            return embed.Build();
        }

        private Embed GetDeletedEmbed(SocketGuild guild, SocketTextChannel channel, MessageLogsMessageRow message)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(245, 66, 66);

            SocketUser user = guild.GetUser(message.UserId);
            string userMention = message.UserId.ToString();

            if (user != null)
            {
                userMention = user.Mention;

                string avatar = user.GetAvatarUrl();
                if (string.IsNullOrEmpty(avatar)) avatar = user.GetDefaultAvatarUrl();

                embed.WithAuthor(new EmbedAuthorBuilder
                {
                    Name = $"{user.Username}#{user.Discriminator}",
                    IconUrl = avatar
                });
            }

            embed.WithDescription($"**Message by {userMention} deleted in {channel.Mention}**");

            if(message.Content.Value.Length > 2024 - embed.Description.Length - 2)
            {
                embed.Description += "\nThe message is too large to fit in this embed";
            }
            else
            {
                embed.Description += $"\n{message.Content}";
            }

            embed.WithFooter($"Message {message.MessageId}");
            embed.WithTimestamp(new DateTimeOffset(message.Timestamp));

            return embed.Build();
        }

        private Embed GetBulkDeletedEmbed(SocketGuild guild, SocketTextChannel channel, int count)
        {
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithColor(245, 66, 66);
            embed.WithDescription($"**{count} messages bulk deleted in {channel.Mention}**\nLogging of bulk deleted messages is not supported yet");

            // TODO: Support logging of bulk deleted messages

            embed.WithAuthor(new EmbedAuthorBuilder
            {
                Name = "Bulk deletion"
            });

            return embed.Build();
        }
    }
}
