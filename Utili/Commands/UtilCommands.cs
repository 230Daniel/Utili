﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Qmmands;
using Utili.Extensions;
using Utili.Implementations;
using Utili.Services;
using Utili.Utils;

namespace Utili.Commands
{
    public class UtilCommands : DiscordGuildModuleBase
    {
        private readonly MemberCacheService _memberCache;
        
        public UtilCommands(MemberCacheService memberCache)
        {
            _memberCache = memberCache;
        }
        
        [Command("Prune", "Purge", "Clear")]
        [RequireAuthorChannelPermissions(Permission.ManageMessages)]
        [RequireBotChannelPermissions(Permission.ManageMessages | Permission.ReadMessageHistory)]
        public async Task Prune()
        {
            await Context.Channel.SendInfoAsync("Prune",
                "Add one or more of the following arguments in any order to delete messages\n" +
                "[amount] - The amount of messages to delete (default 100)\n" +
                "before [message id] - Only messages before a particular message\n" +
                "after [message id] - Only messages after a particular message\n\n" +
                "[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");
        }
        
        [Command("Prune", "Purge", "Clear")]
        [DefaultCooldown(1, 10)]
        [RequireAuthorChannelPermissions(Permission.ManageMessages)]
        [RequireBotChannelPermissions(Permission.ManageMessages | Permission.ReadMessageHistory)]
        public async Task Prune(
            [Remainder]
            string arguments)
        {
            var args = arguments is not null
                ? arguments.Split(" ")
                : Array.Empty<string>();

            uint count = 0;
            var countSet = false;
            var beforeSet = false;
            var afterSet = false;

            IMessage afterMessage = null;
            IMessage beforeMessage = Context.Message;

            for (var i = 0; i < args.Length; i++)
            {
                if (!countSet && uint.TryParse(args[i], out var newCount))
                {
                    count = newCount;
                    countSet = true;
                }
                else
                {
                    switch (args[i].ToLower())
                    {
                        case "before" when !beforeSet:
                            try
                            {
                                i++;
                                var messageId = ulong.Parse(args[i]);
                                beforeMessage = await Context.Channel.FetchMessageAsync(messageId);
                                if (beforeMessage is null) throw new Exception();
                                beforeSet = true;
                                break;
                            }
                            catch
                            {
                                await Context.Channel.SendFailureAsync("Error", $"Invalid message id \"{args[i].ToLower()}\"\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                                return;
                            }

                        case "after" when !afterSet:
                            try
                            {
                                i++;
                                var messageId = ulong.Parse(args[i]);
                                afterMessage = await Context.Channel.FetchMessageAsync(messageId);
                                if (afterMessage is null) throw new Exception();
                                afterSet = true;
                                break;
                            }
                            catch
                            {
                                await Context.Channel.SendFailureAsync("Error", $"Invalid message id \"{args[i].ToLower()}\"\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                                return;
                            }
                            
                        default:
                            await Context.Channel.SendFailureAsync("Error", $"Invalid argument \"{args[i].ToLower()}\"");
                            return;
                    }
                }
            }

            if (!countSet && !beforeSet && !afterSet)
            {
                await Context.Channel.SendInfoAsync("Prune",
                    "Add one or more of the following arguments in any order to delete messages\n" +
                    "[amount] - The amount of messages to delete (default 100)\n" +
                    "before [message id] - Only messages before a particular message\n" +
                    "after [message id] - Only messages after a particular message\n\n" +
                    "[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");

                return;
            }

            if(afterMessage is not null && beforeMessage is not null && afterMessage.CreatedAt() >= beforeMessage.CreatedAt())
            {
                await Context.Channel.SendFailureAsync("Error", "There are no messages between the after and before messages");
                return;
            }

            var content = "";
            bool? premium = null;

            if (!countSet)
            {
                premium = await Premium.IsGuildPremiumAsync(Context.Guild.Id);
                count = premium.Value ? 1000u : 100u;
            }

            if (count > 1000)
            {
                count = 1000;
                premium ??= await Premium.IsGuildPremiumAsync(Context.Guild.Id);
                content = premium.Value
                    ? "For premium servers, you can delete up to 1000 messages at once\n" 
                    : "For non-premium servers, you can delete up to 100 messages at once\n";
            }

            if (count > 100)
            {
                premium ??= await Premium.IsGuildPremiumAsync(Context.Guild.Id);
                if (!premium.Value)
                {
                    count = 100;
                    content = "For non-premium servers, you can delete up to 100 messages at once\n";
                }
            }

            List<IMessage> messages;
            if(afterMessage is not null) messages = (await Context.Channel.FetchMessagesAsync((int)count, RetrievalDirection.After, afterMessage.Id)).ToList();
            else if (beforeMessage is not null) messages = (await Context.Channel.FetchMessagesAsync((int)count,RetrievalDirection.Before, beforeMessage.Id)).ToList();
            else messages = (await Context.Channel.FetchMessagesAsync((int)count)).ToList();

            messages = messages.OrderBy(x => x.CreatedAt().UtcDateTime).ToList();
            if (beforeMessage is not null)
            {
                if (messages.Any(x => x.Id == beforeMessage.Id))
                {
                    var index = messages.FindIndex(x => x.Id == beforeMessage.Id);
                    messages.RemoveRange(index, messages.Count - index);
                }
            }

            var pinned = messages.RemoveAll(x => x is IUserMessage {IsPinned: true});
            var outdated = messages.RemoveAll(x => x.CreatedAt().UtcDateTime < DateTime.UtcNow - TimeSpan.FromDays(13.9));

            if (pinned == 1) content += $"{pinned} message was not deleted because it is pinned\n";
            else if (pinned > 1) content += $"{pinned} messages were not deleted because they are pinned\n";
            if (outdated == 1) content += $"{outdated} message was not deleted because it is older than 14 days\n";
            else if (outdated > 1) content += $"{outdated} messages were not deleted because they are older than 14 days\n";

            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages.Select(x => x.Id), new DefaultRestRequestOptions {Reason = $"Prune (manual by {Context.Message.Author} {Context.Message.Author.Id})"});

            var title = $"{messages.Count} messages deleted";
            if (messages.Count == 1) title = $"{messages.Count} message deleted";

            var sentMessage = await Context.Channel.SendSuccessAsync(title, content);
            await Task.Delay(5000);
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(new[] {sentMessage.Id, Context.Message.Id});
        }

        [Command("React", "AddReaction", "AddEmoji")]
        [DefaultCooldown(2, 5)]
        [RequireAuthorChannelPermissions(Permission.AddReactions | Permission.ManageMessages)]
        [RequireBotChannelPermissions(Permission.AddReactions | Permission.ReadMessageHistory)]
        public async Task React(
            ulong messageId,
            IEmoji emoji)
        {
            var message = await Context.Channel.FetchMessageAsync(messageId);

            if (message is null)
            {
                await Context.Channel.SendFailureAsync("Error", $"No message was found in <#{Context.Channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");
                return;
            }
            
            await message.AddReactionAsync(LocalEmoji.FromEmoji(emoji));
            await Context.Channel.SendSuccessAsync("Reaction added",
                $"The {emoji} reaction was added to a message sent by {message.Author.Mention}");
        }

        [Command("React", "AddReaction", "AddEmoji")]
        [DefaultCooldown(2, 5)]
        public async Task React(
            [RequireAuthorParameterChannelPermissions(Permission.AddReactions | Permission.ManageMessages)]
            [RequireBotParameterChannelPermissions(Permission.AddReactions | Permission.ReadMessageHistory)]
            ITextChannel channel, 
            ulong messageId, 
            IEmoji emoji)
        {
            var message = await channel.FetchMessageAsync(messageId);

            if (message is null)
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"No message was found in <#{channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                return;
            }

            await message.AddReactionAsync(LocalEmoji.FromEmoji(emoji));

            await Context.Channel.SendSuccessAsync("Reaction added",
                $"The {emoji} reaction was added to a message sent by {message.Author.Mention} in {channel.Mention}");
        }

        [Command("Random", "Pick")]
        public async Task Random()
        {
            await _memberCache.TemporarilyCacheMembersAsync(Context.Guild.Id);
            
            var random = new Random();
            var members = Context.Guild.GetMembers().Values.ToList();
            var member = members[random.Next(0, members.Count)];

            await Context.Channel.SendInfoAsync("Random member",
                $"{member.Mention} ({member})\n" +
                $"This member was picked randomly from {members.Count} server member{(members.Count == 1 ? "" : "s")}");
        }
        
        [Command("Random", "Pick")]
        public async Task Random(
            [Remainder]
            IRole role)
        {
            await _memberCache.TemporarilyCacheMembersAsync(Context.Guild.Id);
            
            var random = new Random();
            var members = Context.Guild.GetMembers().Values
                .Where(x => x.RoleIds.Contains(role.Id))
                .ToList();
            var member = members[random.Next(0, members.Count)];

            await Context.Channel.SendInfoAsync("Random member",
                $"{member.Mention} ({member})\n" +
                $"This member was picked randomly from {members.Count} server member{(members.Count == 1 ? "" : "s")} with the {role.Mention} role");
        }
        
        [Command("Random", "Pick")]
        [DefaultCooldown(2, 5)]
        public async Task Random(ITextChannel channel, ulong messageId, IEmoji emoji)
        {
            var message = await channel.FetchMessageAsync(messageId);
            
            if (message is null || !message.Reactions.HasValue)
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"No message was found in {channel.Mention} with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");
                return;
            }
            
            if(message.Reactions.Value.TryGetValue(emoji, out _))
            {
                var reactedMembers = await message.FetchReactionsAsync(LocalEmoji.FromEmoji(emoji), int.MaxValue);
                var random = new Random();
                var member = reactedMembers[random.Next(0, reactedMembers.Count)];

                await Context.Channel.SendInfoAsync("Random member",
                    $"{member.Mention} ({member})\n" +
                    $"This member was picked randomly from {reactedMembers.Count} member{(reactedMembers.Count == 1 ? "" : "s")} " +
                    $"that reacted to [this message]({message.GetJumpUrl(Context.GuildId)}) with {emoji}.");
            }
            else
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"That message doesn't have the {emoji} reaction");
            }
        }
        
        [Command("Random", "Pick")]
        [DefaultCooldown(2, 5)]
        public async Task Random(ulong messageId, IEmoji emoji)
        {
            await Random(Context.Channel as ITextChannel, messageId, emoji);
        }
        
        [Command("WhoHas")]
        public async Task<DiscordCommandResult> WhoHas(
            [Remainder]
            IRole[] roles)
        {
            await _memberCache.TemporarilyCacheMembersAsync(Context.GuildId);
            
            var members = Context.Guild.GetMembers().Values
                .Where(x =>
                {
                    foreach (var role in roles.Where(y => y.Id != Context.Guild.Id))
                        if(!x.RoleIds.Contains(role.Id)) return false;
                    return true;
                })
                .OrderBy(x => x.Nick ?? x.Name)
                .ToList();

            var roleString = string.Join(", ", roles.Select(x => x.Name));
            
            if (members.Count == 0)
            {
                await Context.Channel.SendFailureAsync($"Members with {roleString}", "There are no members with those roles.");
                return null;
            }

            var pages = new List<Page>();
            var content = "";
            var embed = MessageUtils.CreateEmbed(EmbedType.Info, $"Members with {roleString}");

            for (var i = 0; i < members.Count; i++)
            {
                content += $"{members[i].Mention}\n";
                if ((i + 1) % 10 == 0)
                {
                    embed.AddField(new LocalEmbedField()
                        .WithBlankName()
                        .WithValue(content)
                        .WithIsInline(true));
                    content = "";
                }
                if ((i + 1) % 30 == 0)
                {
                    pages.Add(new Page().AddEmbed(embed));
                    embed = MessageUtils.CreateEmbed(EmbedType.Info, "Inactive Members");
                }
            }

            if (!string.IsNullOrWhiteSpace(content))
                embed.AddField(new LocalEmbedField()
                    .WithBlankName()
                    .WithValue(content)
                    .WithIsInline(true));

            if (embed.Fields.Count > 0)
                pages.Add(new Page().AddEmbed(embed));
            
            var pageProvider = new ListPageProvider(pages);
            var menu = new MyPagedView(pageProvider);
            return View(menu);
        }

        [Command("B64Encode")]
        public async Task B64Encode([Remainder] string input)
        {
            var output = input.ToEncoded();
            await Context.Channel.SendSuccessAsync("Encoded string to base 64", output);
        }

        [Command("B64Decode")]
        public async Task B64Decode([Remainder] string input)
        {
            var output = input.ToDecoded();

            if (output == input)
            {
                await Context.Channel.SendFailureAsync("Failed to decode string", "The input string is not valid base 64");
                return;
            }

            await Context.Channel.SendSuccessAsync("Decoded string from base 64", output);
        }
    }
}
