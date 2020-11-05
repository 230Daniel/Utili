using System;
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

            if (row.DeletedChannelId == 0 && row.EditedChannelId == 0) return;

            MessageLogsMessageRow message = new MessageLogsMessageRow()
            {
                GuildId = context.Guild.Id,
                ChannelId = context.Channel.Id,
                MessageId = context.Message.Id,
                Timestamp = DateTime.UtcNow,
                Content = context.Message.Content
            };

            Database.Data.MessageLogs.SaveMessage(message);
            Database.Data.MessageLogs.DeleteOldMessages(context.Guild.Id, context.Channel.Id,
                Premium.IsPremium(context.Guild.Id));
        }

        public async Task MessageEdited(SocketCommandContext context)
        {
            MessageLogsRow row = Database.Data.MessageLogs.GetRow(context.Guild.Id);
            if (row.DeletedChannelId == 0 && row.EditedChannelId == 0) return;

            MessageLogsMessageRow message = Database.Data.MessageLogs.GetMessage(context.Guild.Id, context.Channel.Id, context.Message.Id);
            if (message.Content == context.Message.Content) return;

            Embed embed = GetEditedEmbed(message, context);

            message.Content = context.Message.Content;
            Database.Data.MessageLogs.SaveMessage(message);

            SocketTextChannel channel = context.Guild.GetTextChannel(row.EditedChannelId);
            if(channel == null) return;
            await channel.SendMessageAsync(embed: embed);
        }

        public async Task MessageDeleted(SocketGuild guild, SocketTextChannel channel, ulong messageId)
        {
            MessageLogsRow row = Database.Data.MessageLogs.GetRow(guild.Id);
            if (row.DeletedChannelId == 0 && row.EditedChannelId == 0) return;

            MessageLogsMessageRow message = Database.Data.MessageLogs.GetMessage(guild.Id, channel.Id, messageId);

            Embed embed = GetDeletedEmbed(guild, channel, message);

            Database.Data.MessageLogs.DeleteMessagesById(new [] {message.Id});

            SocketTextChannel logChannel = guild.GetTextChannel(row.DeletedChannelId);
            if(logChannel == null) return;
            await logChannel.SendMessageAsync(embed: embed);
        }

        private Embed GetEditedEmbed(MessageLogsMessageRow before, SocketCommandContext after)
        {
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithDescription($"**Message by {after.User.Mention} edited in {after.Channel}** [Jump]({after.Message.GetJumpUrl()})");

            if(before == null)
            {
                embed.Description += $"\nThe content of the message could not be retrieved";
            }
            else if(before.Content.Length > 1024 || after.Message.Content.Length > 1024)
            {
                if(before.Content.Length < 2024 - embed.Description.Length - 2)
                {
                    embed.Description += $"\n{before.Content}";
                }
                else
                {
                    embed.Description += "\nThe message is too large to fit in this embed";
                }

                embed.WithFooter("Sent");
                embed.WithTimestamp(new DateTimeOffset(before.Timestamp));
            }
            else
            {
                embed.AddField("Before", before.Content);
                embed.AddField("After", after.Message.Content);

                embed.WithFooter("Sent");
                embed.WithTimestamp(new DateTimeOffset(before.Timestamp));
            }

            embed.WithColor(66, 182, 245);

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

            if(message == null)
            {
                embed.WithDescription(
                    $"**Message deleted in {channel.Mention}**\nThe content of the message could not be retrieved");
            }
            else
            {
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

                if(message.Content.Length > 2024 - embed.Description.Length - 2)
                {
                    embed.Description += "\nThe message is too large to fit in this embed";
                }
                else
                {
                    embed.Description += $"\n{message.Content}";
                }

                embed.WithFooter("Sent");
                embed.WithTimestamp(new DateTimeOffset(message.Timestamp));
            }

            return embed.Build();
        }
    }
}
