using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Qmmands;
using Qmmands.Text;
using Utili.Bot.Implementations;
using Utili.Bot.Services;
using Utili.Bot.Utils;
using Utili.Bot.Extensions;

namespace Utili.Bot.Commands;

public class UtilCommands : MyDiscordTextGuildModuleBase
{
    private readonly MemberCacheService _memberCache;
    private readonly IsPremiumService _isPremiumService;

    public UtilCommands(MemberCacheService memberCache, IsPremiumService isPremiumService)
    {
        _memberCache = memberCache;
        _isPremiumService = isPremiumService;
    }

    [TextCommand("prune", "purge", "clear")]
    [RequireAuthorPermissions(Permissions.ManageMessages)]
    [RequireBotPermissions(Permissions.ManageMessages | Permissions.ReadMessageHistory)]
    public IResult Prune()
    {
        return Info("Prune",
            "Add one or more of the following arguments in any order to delete messages\n" +
            "[amount] - The amount of messages to delete (default 100)\n" +
            "before [message id] - Only messages before a particular message\n" +
            "after [message id] - Only messages after a particular message\n\n" +
            "[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");
    }

    [TextCommand("prune", "purge", "clear")]
    [DefaultRateLimit(1, 10)]
    [RequireAuthorPermissions(Permissions.ManageMessages)]
    [RequireBotPermissions(Permissions.ManageMessages | Permissions.ReadMessageHistory)]
    public async Task<IResult> PruneAsync(
        [Remainder] string arguments)
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
                            beforeMessage = await Context.GetChannel().FetchMessageAsync(messageId);
                            if (beforeMessage is null) throw new Exception();
                            beforeSet = true;
                            break;
                        }
                        catch
                        {
                            return Failure("Error", $"Invalid message id \"{args[i].ToLower()}\"\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                        }

                    case "after" when !afterSet:
                        try
                        {
                            i++;
                            var messageId = ulong.Parse(args[i]);
                            afterMessage = await Context.GetChannel().FetchMessageAsync(messageId);
                            if (afterMessage is null) throw new Exception();
                            afterSet = true;
                            break;
                        }
                        catch
                        {
                            return Failure("Error", $"Invalid message id \"{args[i].ToLower()}\"\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                        }

                    default:
                        return Failure("Error", $"Invalid argument \"{args[i].ToLower()}\"");
                }
            }
        }

        if (!countSet && !beforeSet && !afterSet)
        {
            return Info("Prune",
                "Add one or more of the following arguments in any order to delete messages\n" +
                "[amount] - The amount of messages to delete (default 100)\n" +
                "before [message id] - Only messages before a particular message\n" +
                "after [message id] - Only messages after a particular message\n\n" +
                "[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");
        }

        if (afterMessage is not null && beforeMessage is not null && afterMessage.CreatedAt() >= beforeMessage.CreatedAt())
        {
            return Failure("Error", "There are no messages between the after and before messages");
        }

        var content = "";
        bool? premium = null;

        if (!countSet)
        {
            premium = await _isPremiumService.GetIsGuildPremiumAsync(Context.GuildId);
            count = premium.Value ? 1000u : 100u;
        }

        if (count > 1000)
        {
            count = 1000;
            premium ??= await _isPremiumService.GetIsGuildPremiumAsync(Context.GuildId);
            content = premium.Value
                ? "For premium servers, you can delete up to 1000 messages at once\n"
                : "For non-premium servers, you can delete up to 100 messages at once\n";
        }

        if (count > 100)
        {
            premium ??= await _isPremiumService.GetIsGuildPremiumAsync(Context.GuildId);
            if (!premium.Value)
            {
                count = 100;
                content = "For non-premium servers, you can delete up to 100 messages at once\n";
            }
        }

        List<IMessage> messages;
        if (afterMessage is not null) messages = (await Context.GetChannel().FetchMessagesAsync((int)count, FetchDirection.After, afterMessage.Id)).ToList();
        else if (beforeMessage is not null) messages = (await Context.GetChannel().FetchMessagesAsync((int)count, FetchDirection.Before, beforeMessage.Id)).ToList();
        else messages = (await Context.GetChannel().FetchMessagesAsync((int)count)).ToList();

        messages = messages.OrderBy(x => x.CreatedAt().UtcDateTime).ToList();
        if (beforeMessage is not null)
        {
            if (messages.Any(x => x.Id == beforeMessage.Id))
            {
                var index = messages.FindIndex(x => x.Id == beforeMessage.Id);
                messages.RemoveRange(index, messages.Count - index);
            }
        }

        var pinned = messages.RemoveAll(x => x is IUserMessage { IsPinned: true });
        var outdated = messages.RemoveAll(x => x.CreatedAt().UtcDateTime < DateTime.UtcNow - TimeSpan.FromDays(13.9));

        if (pinned == 1) content += $"{pinned} message was not deleted because it is pinned\n";
        else if (pinned > 1) content += $"{pinned} messages were not deleted because they are pinned\n";
        if (outdated == 1) content += $"{outdated} message was not deleted because it is older than 14 days\n";
        else if (outdated > 1) content += $"{outdated} messages were not deleted because they are older than 14 days\n";

        await Context.GetChannel().DeleteMessagesAsync(messages.Select(x => x.Id), new DefaultRestRequestOptions { Reason = $"Prune (manual by {Context.Message.Author} {Context.Message.Author.Id})" });

        var title = $"{messages.Count} messages deleted";
        if (messages.Count == 1) title = $"{messages.Count} message deleted";

        var sentMessage = await Context.GetChannel().SendSuccessAsync(title, content);
        await Task.Delay(5000);
        await Context.GetChannel().DeleteMessagesAsync(new[] { sentMessage.Id, Context.Message.Id });

        return null;
    }

    [TextCommand("react", "addreaction", "addemoji")]
    [DefaultRateLimit(2, 5)]
    [RequireAuthorPermissions(Permissions.AddReactions | Permissions.ManageMessages)]
    [RequireBotPermissions(Permissions.AddReactions | Permissions.ReadMessageHistory)]
    public async Task<IResult> ReactAsync(
        ulong messageId,
        IEmoji emoji)
    {
        var message = await Context.GetChannel().FetchMessageAsync(messageId);

        if (message is null)
            return Failure("Error", $"No message was found in <#{Context.ChannelId}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");

        await message.AddReactionAsync(LocalEmoji.FromEmoji(emoji));
        return Success("Reaction added",
            $"The {emoji} reaction was added to a message sent by {message.Author.Mention}");
    }

    [TextCommand("react", "addreaction", "addemoji")]
    [DefaultRateLimit(2, 5)]
    public async Task<IResult> ReactAsync(
        [RequireAuthorParameterChannelPermissions(Permissions.AddReactions | Permissions.ManageMessages)]
        [RequireBotParameterChannelPermissions(Permissions.AddReactions | Permissions.ReadMessageHistory)]
        IMessageGuildChannel channel,
        ulong messageId,
        IEmoji emoji)
    {
        var message = await channel.FetchMessageAsync(messageId);

        if (message is null)
            return Failure("Error",
                $"No message was found in <#{channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");

        await message.AddReactionAsync(LocalEmoji.FromEmoji(emoji));

        return Success("Reaction added",
            $"The {emoji} reaction was added to a message sent by {message.Author.Mention} in {channel.Mention}");
    }

    [TextCommand("random", "pick")]
    public async Task<IResult> RandomAsync()
    {
        await _memberCache.TemporarilyCacheMembersAsync(Context.GuildId);

        var random = new Random();
        var members = Context.GetGuild().GetMembers().Values.ToList();
        var member = members[random.Next(0, members.Count)];

        return Info("Random member",
            $"{member.Mention} ({member})\n" +
            $"This member was picked randomly from {members.Count} server member{(members.Count == 1 ? "" : "s")}");
    }

    [TextCommand("random", "pick")]
    public async Task<IResult> RandomAsync(
        [Remainder] IRole role)
    {
        await _memberCache.TemporarilyCacheMembersAsync(Context.GuildId);

        var random = new Random();
        var members = Context.GetGuild().GetMembers().Values
            .Where(x => x.RoleIds.Contains(role.Id))
            .ToList();
        var member = members[random.Next(0, members.Count)];

        return Info("Random member",
            $"{member.Mention} ({member})\n" +
            $"This member was picked randomly from {members.Count} server member{(members.Count == 1 ? "" : "s")} with the {role.Mention} role");
    }

    [TextCommand("random", "pick")]
    [DefaultRateLimit(2, 5)]
    public async Task<IResult> RandomAsync(IMessageGuildChannel channel, ulong messageId, IEmoji emoji)
    {
        var message = await channel.FetchMessageAsync(messageId);

        if (message is null || !message.Reactions.HasValue)
            return Failure("Error",
                $"No message was found in {channel.Mention} with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");

        if (message.Reactions.Value.TryGetValue(emoji, out _))
        {
            var reactedMembers = await message.FetchReactionsAsync(LocalEmoji.FromEmoji(emoji), int.MaxValue);
            var random = new Random();
            var member = reactedMembers[random.Next(0, reactedMembers.Count)];

            return Info("Random member",
                $"{member.Mention} ({member})\n" +
                $"This member was picked randomly from {reactedMembers.Count} member{(reactedMembers.Count == 1 ? "" : "s")} " +
                $"that reacted to [this message]({message.GetJumpUrl(Context.GuildId)}) with {emoji}.");
        }

        return Failure("Error",
            $"That message doesn't have the {emoji} reaction");
    }

    [TextCommand("random", "pick")]
    [DefaultRateLimit(2, 5)]
    public Task<IResult> RandomAsync(ulong messageId, IEmoji emoji)
    {
        return RandomAsync(Context.GetChannel(), messageId, emoji);
    }

    [TextCommand("whohas")]
    public async Task<IResult> WhoHasAsync(
        [Remainder] IRole[] roles)
    {
        await _memberCache.TemporarilyCacheMembersAsync(Context.GuildId);

        var members = Context.GetGuild().GetMembers().Values
            .Where(x =>
            {
                foreach (var role in roles.Where(y => y.Id != Context.GuildId))
                    if (!x.RoleIds.Contains(role.Id))
                        return false;
                return true;
            })
            .OrderBy(x => x.Nick ?? x.Name)
            .ToList();

        var roleString = string.Join(", ", roles.Select(x => x.Name));

        if (members.Count == 0)
            return Failure($"Members with {roleString}", "There are no members with those roles.");

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
                    .WithIsInline());
                content = "";
            }
            if ((i + 1) % 30 == 0)
            {
                pages.Add(new Page().AddEmbed(embed));
                embed = MessageUtils.CreateEmbed(EmbedType.Info, $"Members with {roleString}");
            }
        }

        if (!string.IsNullOrWhiteSpace(content))
            embed.AddField(new LocalEmbedField()
                .WithBlankName()
                .WithValue(content)
                .WithIsInline());

        if (embed.Fields.HasValue && embed.Fields.Value.Count > 0)
            pages.Add(new Page().AddEmbed(embed));

        var pageProvider = new ListPageProvider(pages);
        var menu = new MyPagedView(pageProvider);
        return View(menu, TimeSpan.FromMinutes(5));
    }

    [TextCommand("b64encode")]
    public IResult B64Encode([Remainder] string input)
    {
        var output = input.ToEncoded();
        return Success("Encoded string to base 64", output);
    }

    [TextCommand("b64decode")]
    public IResult B64Decode([Remainder] string input)
    {
        var output = input.ToDecoded();

        if (output == input)
            return Failure("Failed to decode string", "The input string is not valid base 64");

        return Success("Decoded string from base 64", output);
    }
}
