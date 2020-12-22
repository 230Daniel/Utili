using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Utili.Commands;
using static Utili.MessageSender;
using static Utili.Program;

namespace Utili.Features
{
    internal static class Reputation
    {
        public static async Task ReactionAdded(IGuild guild, IUserMessage message, IUser reactor, IEmote emote)
        {
            IUser user = message.Author;
            if(user.Id == reactor.Id || user.IsBot || reactor.IsBot) return;

            ReputationRow row = await Database.Data.Reputation.GetRowAsync(guild.Id);
            if (!row.Emotes.Any(x => x.Item1.Equals(emote))) return;
            int change = row.Emotes.First(x => Equals(x.Item1, emote)).Item2;

            await Database.Data.Reputation.AlterUserReputationAsync(guild.Id, user.Id, change);
        }

        public static async Task ReactionRemoved(IGuild guild, IUserMessage message, IUser reactor, IEmote emote)
        {
            IUser user = message.Author;
            if(user.Id == reactor.Id) return;

            ReputationRow row = await Database.Data.Reputation.GetRowAsync(guild.Id);
            if (!row.Emotes.Any(x => x.Item1.Equals(emote))) return;
            int change = row.Emotes.First(x => Equals(x.Item1, emote)).Item2;
            change *= -1;

            await Database.Data.Reputation.AlterUserReputationAsync(guild.Id, user.Id, change);
        }
    }

    [Group("Reputation"), Alias("Rep")]
    public class ReputationCommands : ModuleBase<SocketCommandContext>
    {
        [Command("")]
        public async Task Reputation(IUser user = null)
        {
            if (user == null) user = Context.User;

            ReputationUserRow row = await Database.Data.Reputation.GetUserRowAsync(Context.Guild.Id, user.Id);

            Color colour;
            if (row.Reputation == 0) colour = new Color(195, 195, 195);
            else if (row.Reputation > 0) colour = new Color(67, 181, 129);
            else colour = new Color(181, 67, 67);

            await SendInfoAsync(Context.Channel, null, $"{user.Mention}'s reputation: **{row.Reputation}**", colour: colour);
        }

        [Command("Leaderboard"), Alias("Top"), Cooldown(10)]
        public async Task Leaderboard()
        {
            List<ReputationUserRow> rows = await Database.Data.Reputation.GetUserRowsAsync(Context.Guild.Id);
            rows = rows.OrderByDescending(x => x.Reputation).ToList();

            List<IGuildUser> users = Context.Guild.Users.Select(x => x as IGuildUser).ToList();
            if(users.Count < Context.Guild.MemberCount) users = (await Context.Guild.GetUsersAsync().FlattenAsync()).ToList();
            List<ulong> userIds = users.Select(x => x.Id).ToList();

            int position = 1;
            string content = "";
            foreach (ReputationUserRow row in rows)
            {
                if (userIds.Contains(row.UserId))
                {
                    IGuildUser user = users.First(x => x.Id == row.UserId);
                    content += $"{position}. {user.Mention} {row.Reputation}\n";
                    if(position == 10) break;
                    position++;
                }
            }

            await SendInfoAsync(Context.Channel, "Reputation Leaderboard", content);
        }

        [Command("InverseLeaderboard"), Alias("Bottom"), Cooldown(10)]
        public async Task InvserseLeaderboard()
        {
            List<ReputationUserRow> rows = await Database.Data.Reputation.GetUserRowsAsync(Context.Guild.Id);
            rows = rows.OrderBy(x => x.Reputation).ToList();

            List<IGuildUser> users = Context.Guild.Users.Select(x => x as IGuildUser).ToList();
            if(users.Count < Context.Guild.MemberCount) users = (await Context.Guild.GetUsersAsync().FlattenAsync()).ToList();
            List<ulong> userIds = users.Select(x => x.Id).ToList();

            int position = rows.Count;
            string content = "";
            foreach (ReputationUserRow row in rows)
            {
                if (userIds.Contains(row.UserId))
                {
                    IGuildUser user = users.First(x => x.Id == row.UserId);
                    content += $"{position}. {user.Mention} {row.Reputation}\n";
                    if(position == rows.Count - 10) break;
                    position--;
                }
            }

            await SendInfoAsync(Context.Channel, $"Inverse Reputation Leaderboard", content);
        }

        [Command("Give"), Permission(Perm.ManageGuild), Cooldown(2)]
        public async Task Give(IUser user, ulong change)
        {
            await Database.Data.Reputation.AlterUserReputationAsync(Context.Guild.Id, user.Id, (long)change);
            await SendSuccessAsync(Context.Channel, "Reputation given", $"Gave {change} reputation to {user.Mention}");
        }

        [Command("Take"), Permission(Perm.ManageGuild), Cooldown(2)]
        public async Task Take(IUser user, ulong change)
        {
            await Database.Data.Reputation.AlterUserReputationAsync(Context.Guild.Id, user.Id, -(long)change);
            await SendSuccessAsync(Context.Channel, "Reputation taken", $"Took {change} reputation from {user.Mention}");
        }

        [Command("Set"), Permission(Perm.ManageGuild), Cooldown(2)]
        public async Task Take(IUser user, long amount)
        {
            await Database.Data.Reputation.SetUserReputationAsync(Context.Guild.Id, user.Id, amount);
            await SendSuccessAsync(Context.Channel, "Reputation set", $"Set {user.Mention}'s reputation to {amount}");
        }

        [Command("AddEmote"), Permission(Perm.ManageGuild), Cooldown(2)]
        public async Task AddEmote(string emoteString, int value = 0)
        {
            IEmote emote = Helper.GetEmote(emoteString);
            ReputationRow row = await Database.Data.Reputation.GetRowAsync(Context.Guild.Id);

            if (row.Emotes.Any(x => x.Item1 == emote))
            {
                await SendFailureAsync(Context.Channel, "Error",
                    "That emote is already added");
                return;
            }

            try
            {
                await Context.Message.AddReactionAsync(emote);
            }
            catch
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"An emote was not found matching {emoteString}");
                return;
            }

            await Context.Message.RemoveReactionAsync(emote, _client.CurrentUser);

            row.Emotes.Add((emote, value));
            await Database.Data.Reputation.SaveRowAsync(row);

            await SendSuccessAsync(Context.Channel, "Emote added", 
                $"The {emote} emote was added successfully with value {value}\nYou can change its value or remove it on the dashboard");
        }
    }
}
