using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class ReputationService
    {
        ILogger<ReputationService> _logger;
        DiscordClientBase _client;

        public ReputationService(ILogger<ReputationService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public Task ReactionAdded(object sender, ReactionAddedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if(!e.GuildId.HasValue) return;

                    IGuild guild = _client.GetGuild(e.GuildId.Value);
                    ITextChannel channel = guild.GetTextChannel(e.ChannelId);

                    ReputationRow row = await Reputation.GetRowAsync(e.GuildId.Value);
                    if (!row.Emotes.Any(x => Helper.GetEmoji(x.Item1, guild).Equals(e.Emoji))) return;
                    int change = row.Emotes.First(x => Equals(x.Item1, e.Emoji.ToString())).Item2;

                    IUserMessage message = e.Message ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
                    if(message is null || message.Author.IsBot || message.Author.Id == e.UserId) return;

                    IMember reactor = e.Member ?? await guild.FetchMemberAsync(e.UserId);
                    if(reactor is null || reactor.IsBot) return;

                    await Reputation.AlterUserReputationAsync(guild.Id, message.Author.Id, change);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on reaction added");
                }
            });
            return Task.CompletedTask;
        }

        public Task ReactionRemoved(object sender, ReactionRemovedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if(!e.GuildId.HasValue) return;

                    IGuild guild = _client.GetGuild(e.GuildId.Value);
                    ITextChannel channel = guild.GetTextChannel(e.ChannelId);

                    ReputationRow row = await Reputation.GetRowAsync(e.GuildId.Value);
                    if (!row.Emotes.Any(x => Helper.GetEmoji(x.Item1, guild).Equals(e.Emoji))) return;
                    int change = -1 * row.Emotes.First(x => Equals(x.Item1, e.Emoji.ToString())).Item2;

                    IUserMessage message = e.Message ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
                    if(message is null || message.Author.IsBot || message.Author.Id == e.UserId) return;

                    IMember reactor = await guild.FetchMemberAsync(e.UserId);
                    if(reactor is null || reactor.IsBot) return;

                    await Reputation.AlterUserReputationAsync(guild.Id, message.Author.Id, change);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on reaction removed");
                }
            });
            return Task.CompletedTask;
        }
    }

    //[Discord.Commands.Group("Reputation"), Alias("Rep")]
    //public class ReputationCommands : DiscordGuildModuleBase
    //{
    //    [Discord.Commands.Command("")]
    //    public async Task Reputation(IUser user = null)
    //    {
    //        //user ??= Context.User;

    //        ReputationUserRow row = await Database.Data.Reputation.GetUserRowAsync(Context.Guild.Id, user.Id);

    //        Color colour;
    //        if (row.Reputation == 0) colour = new Color(195, 195, 195);
    //        else if (row.Reputation > 0) colour = new Color(67, 181, 129);
    //        else colour = new Color(181, 67, 67);

    //        await SendInfoAsync(Context.Channel, null, $"{user.Mention}'s reputation: **{row.Reputation}**", colour: colour);
    //    }

    //    [Discord.Commands.Command("Leaderboard"), Alias("Top"), DefaultCooldown(1, 5)]
    //    public async Task Leaderboard()
    //    {
    //        await Context.Guild.DownloadAndKeepUsersAsync(TimeSpan.FromMinutes(15));
    //        List<ReputationUserRow> rows = await Database.Data.Reputation.GetUserRowsAsync(Context.Guild.Id);
    //        rows = rows.OrderByDescending(x => x.Reputation).ToList();

    //        List<ulong> userIds = Context.Guild.Users.Select(x => x.Id).ToList();

    //        int position = 1;
    //        string content = "";
    //        foreach (ReputationUserRow row in rows)
    //        {
    //            if (userIds.Contains(row.UserId))
    //            {
    //                IGuildUser user = Context.Guild.Users.First(x => x.Id == row.UserId);
    //                content += $"{position}. {user.Mention} {row.Reputation}\n";
    //                if(position == 10) break;
    //                position++;
    //            }
    //        }

    //        await SendInfoAsync(Context.Channel, "Reputation Leaderboard", content);
    //    }

    //    [Discord.Commands.Command("InverseLeaderboard"), Alias("Bottom"), Cooldown(10)]
    //    public async Task InvserseLeaderboard()
    //    {
    //        await Context.Guild.DownloadAndKeepUsersAsync(TimeSpan.FromMinutes(15));
    //        List<ReputationUserRow> rows = await Database.Data.Reputation.GetUserRowsAsync(Context.Guild.Id);
    //        rows = rows.OrderBy(x => x.Reputation).ToList();

    //        List<ulong> userIds = Context.Guild.Users.Select(x => x.Id).ToList();

    //        int position = rows.Count;
    //        string content = "";
    //        foreach (ReputationUserRow row in rows)
    //        {
    //            if (userIds.Contains(row.UserId))
    //            {
    //                IGuildUser user = Context.Guild.Users.First(x => x.Id == row.UserId);
    //                content += $"{position}. {user.Mention} {row.Reputation}\n";
    //                if(position == rows.Count - 10) break;
    //                position--;
    //            }
    //        }

    //        await SendInfoAsync(Context.Channel, "Inverse Reputation Leaderboard", content);
    //    }

    //    [Discord.Commands.Command("Give"), Permission(Perm.ManageGuild), Cooldown(2)]
    //    public async Task Give(IUser user, ulong change)
    //    {
    //        await Database.Data.Reputation.AlterUserReputationAsync(Context.Guild.Id, user.Id, (long)change);
    //        await Context.Channel.SendSuccessAsync("Reputation given", $"Gave {change} reputation to {user.Mention}");
    //    }

    //    [Discord.Commands.Command("Take"), Permission(Perm.ManageGuild), Cooldown(2)]
    //    public async Task Take(IUser user, ulong change)
    //    {
    //        await Database.Data.Reputation.AlterUserReputationAsync(Context.Guild.Id, user.Id, -(long)change);
    //        await Context.Channel.SendSuccessAsync("Reputation taken", $"Took {change} reputation from {user.Mention}");
    //    }

    //    [Discord.Commands.Command("Set"), Permission(Perm.ManageGuild), Cooldown(2)]
    //    public async Task Take(IUser user, long amount)
    //    {
    //        await Database.Data.Reputation.SetUserReputationAsync(Context.Guild.Id, user.Id, amount);
    //        await Context.Channel.SendSuccessAsync("Reputation set", $"Set {user.Mention}'s reputation to {amount}");
    //    }

    //    [Discord.Commands.Command("AddEmoji"), Permission(Perm.ManageGuild), Cooldown(2)]
    //    public async Task AddEmoji(string emoteString, int value = 0)
    //    {
    //        IEmote emote = Helper.GetEmoji(emoteString, Context.Guild);
    //        ReputationRow row = await Database.Data.Reputation.GetRowAsync(Context.Guild.Id);

    //        bool triedAlternative = false;
    //        while (true)
    //        {
    //            try
    //            {
    //                await Context.Message.AddReactionAsync(emote);
    //                break;
    //            }
    //            catch
    //            {
    //                if (!triedAlternative && Context.Guild.Emotes.Any(x => x.Name == emoteString || $":{x.Name}:" == emoteString))
    //                {
    //                    triedAlternative = true;
    //                    emote = Context.Guild.Emotes.First(x => x.Name == emoteString || $":{x.Name}:" == emoteString);
    //                }
    //                else
    //                {
    //                    await Context.Channel.SendFailureAsync("Error", $"An emoji was not found matching {emoteString}");
    //                    return;
    //                }
    //            }
    //        }

    //        _ = Task.Run(async () =>
    //        {
    //            // Rate limit is 1 per 250ms. Stupid but what can you do?
    //            await Task.Delay(500);
    //            await Context.Message.RemoveReactionAsync(emote, _oldClient.CurrentUser);
    //        });

    //        if (row.Emotes.Any(x => Equals(x.Item1, emote)))
    //        {
    //            await Context.Channel.SendFailureAsync("Error", "That emoji is already added");
    //            return;
    //        }

    //        row.Emotes.Add((emote, value));
    //        await Database.Data.Reputation.SaveRowAsync(row);

    //        await Context.Channel.SendSuccessAsync("Emoji added", 
    //            $"The {emote} emoji was added successfully with value {value}\nYou can change its value or remove it on the dashboard");
    //    }
}
