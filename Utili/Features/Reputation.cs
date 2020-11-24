using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Utili.Commands;
using static Utili.MessageSender;
using static Utili.Program;

namespace Utili.Features
{
    internal class Reputation
    {
        public async Task ReactionAdded(IUserMessage message, SocketGuildUser reactor, IEmote emote)
        {
            SocketGuild guild = reactor.Guild;
            IUser user = message.Author;
            if(user.Id == reactor.Id || user.IsBot || reactor.IsBot) return;

            List<ReputationRow> rows = Database.Data.Reputation.GetRows(guild.Id);
            if(rows.Count == 0) return;
            ReputationRow row = rows.First();

            if (!row.Emotes.Select(x => x.Item1).Contains(emote)) return;
            int change = row.Emotes.First(x => Equals(x.Item1, emote)).Item2;

            Database.Data.Reputation.AlterUserReputation(guild.Id, user.Id, change);
        }

        public async Task ReactionRemoved(IUserMessage message, SocketGuildUser reactor, IEmote emote)
        {
            SocketGuild guild = reactor.Guild;
            IUser user = message.Author;
            if(user.Id == reactor.Id) return;

            List<ReputationRow> rows = Database.Data.Reputation.GetRows(guild.Id);
            if(rows.Count == 0) return;
            ReputationRow row = rows.First();

            if (!row.Emotes.Select(x => x.Item1).Contains(emote)) return;
            int change = row.Emotes.First(x => Equals(x.Item1, emote)).Item2;
            change *= -1;

            Database.Data.Reputation.AlterUserReputation(guild.Id, user.Id, change);
        }
    }

    [Group("Reputation"), Alias("Rep")]
    public class ReputationCommands : ModuleBase<SocketCommandContext>
    {
        [Command("")]
        public async Task Reputation(IUser user) // TODO: Write or yoink an IUser typereader
        {
            int reputation = 0;
            List<ReputationUserRow> rows = Database.Data.Reputation.GetUserRows(Context.Guild.Id, user.Id);
            if (rows.Count > 0) reputation = rows.First().Reputation;

            Color colour;
            if (reputation == 0) colour = new Color(195, 195, 195);
            else if (reputation > 0) colour = new Color(67, 181, 129);
            else colour = new Color(181, 67, 67);

            await SendInfoAsync(Context.Channel, null, $"{user.Mention}'s reputation: **{reputation}**", colour: colour);
        }

        [Command("Leaderboard"), Alias("Top"), Cooldown(10)]
        public async Task Leaderboard()
        {
            List<ReputationUserRow> rows = Database.Data.Reputation.GetUserRows(Context.Guild.Id);
            rows = rows.OrderByDescending(x => x.Reputation).ToList();

            List<IGuildUser> users = Context.Guild.Users.Select(x => x as IGuildUser).ToList();
            if(users.Count < Context.Guild.DownloadedMemberCount) users = (await Context.Guild.GetUsersAsync().FlattenAsync()).ToList();
            List<ulong> userIds = users.Select(x => x.Id).ToList();

            int position = 1;
            string content = "";
            foreach (ReputationUserRow row in rows)
            {
                if (userIds.Contains(row.UserId))
                {
                    IGuildUser user = users.First(x => x.Id == row.UserId);
                    content += $"{position}. {user.Mention}: {row.Reputation}\n";
                    if(position == 10) break;
                    position++;
                }
            }

            await SendInfoAsync(Context.Channel, $"Reputation Leaderboard", content);
        }

        [Command("InverseLeaderboard"), Alias("Bottom"), Cooldown(10)]
        public async Task InvserseLeaderboard()
        {
            List<ReputationUserRow> rows = Database.Data.Reputation.GetUserRows(Context.Guild.Id);
            rows = rows.OrderBy(x => x.Reputation).ToList();

            List<IGuildUser> users = Context.Guild.Users.Select(x => x as IGuildUser).ToList();
            if(users.Count < Context.Guild.DownloadedMemberCount) users = (await Context.Guild.GetUsersAsync().FlattenAsync()).ToList();
            List<ulong> userIds = users.Select(x => x.Id).ToList();

            int position = rows.Count;
            string content = "";
            foreach (ReputationUserRow row in rows)
            {
                if (userIds.Contains(row.UserId))
                {
                    IGuildUser user = users.First(x => x.Id == row.UserId);
                    content += $"{position}. {user.Mention}: {row.Reputation}\n";
                    if(position == rows.Count - 10) break;
                    position--;
                }
            }

            await SendInfoAsync(Context.Channel, $"Inverse Reputation Leaderboard", content);
        }

        [Command("Give"), Permission(Perm.ManageGuild), Cooldown(2)]
        public async Task Give(IUser user, uint change)
        {
            Database.Data.Reputation.AlterUserReputation(Context.Guild.Id, user.Id, (int)change);
            await SendSuccessAsync(Context.Channel, "Reputation given", $"Gave {change} reputation to {user.Mention}");
        }

        [Command("Take"), Permission(Perm.ManageGuild), Cooldown(2)]
        public async Task Take(IUser user, uint change)
        {
            Database.Data.Reputation.AlterUserReputation(Context.Guild.Id, user.Id, -(int)change);
            await SendSuccessAsync(Context.Channel, "Reputation taken", $"Took {change} reputation from {user.Mention}");
        }

        [Command("Set"), Permission(Perm.ManageGuild), Cooldown(2)]
        public async Task Take(IUser user, int amount)
        {
            Database.Data.Reputation.SetUserReputation(Context.Guild.Id, user.Id, amount);
            await SendSuccessAsync(Context.Channel, "Reputation set", $"Set {user.Mention}'s reputation to {amount}");
        }
    }
}
