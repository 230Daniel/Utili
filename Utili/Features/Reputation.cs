using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Features
{
    internal static class Reputation
    {
        public static async Task ReactionAdded(IGuild guild, Cacheable<IUserMessage, ulong> partialMessage, ulong reactorId, ISocketMessageChannel channel, IEmote emote)
        {
            ReputationRow row = await Database.Data.Reputation.GetRowAsync(guild.Id);
            if (!row.Emotes.Any(x => x.Item1.Equals(emote))) return;
            int change = row.Emotes.First(x => Equals(x.Item1, emote)).Item2;

            IUserMessage message = await partialMessage.GetOrDownloadAsync();
            IUser user = message.Author;
            IUser reactor = await _oldRest.GetGuildUserAsync(guild.Id, reactorId);

            if(user.Id == reactorId || user.IsBot || reactor.IsBot) return;

            await Database.Data.Reputation.AlterUserReputationAsync(guild.Id, user.Id, change);
        }

        public static async Task ReactionRemoved(IGuild guild, Cacheable<IUserMessage, ulong> partialMessage, ulong reactorId, ISocketMessageChannel channel, IEmote emote)
        {
            ReputationRow row = await Database.Data.Reputation.GetRowAsync(guild.Id);
            if (!row.Emotes.Any(x => x.Item1.Equals(emote))) return;
            int change = row.Emotes.First(x => Equals(x.Item1, emote)).Item2;

            IUserMessage message = await partialMessage.GetOrDownloadAsync();
            IUser user = message.Author;
            IUser reactor = await _oldRest.GetGuildUserAsync(guild.Id, reactorId);

            if(user.Id == reactorId || user.IsBot || reactor.IsBot) return;

            change *= -1;
            await Database.Data.Reputation.AlterUserReputationAsync(guild.Id, user.Id, change);
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
