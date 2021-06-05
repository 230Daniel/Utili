using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Qmmands;
using Utili.Extensions;
using Utili.Implementations;
using Utili.Utils;

namespace Utili.Commands
{
    [Group("Reputation", "Rep")]
    public class RepuatationCommands : DiscordGuildModuleBase
    {
        [Command("")]
        public async Task Reputation(
            [RequireNotBot]
            IMember member = null)
        {
            member ??= Context.Message.Author as IMember;

            ReputationUserRow row = await Database.Data.Reputation.GetUserRowAsync(Context.Guild.Id, member.Id);

            Color colour;
            if (row.Reputation == 0) colour = new Color(195, 195, 195);
            else if (row.Reputation > 0) colour = new Color(67, 181, 129);
            else colour = new Color(181, 67, 67);

            LocalEmbed embed = MessageUtils
                .CreateEmbed(EmbedType.Info, "", $"{member.Mention}'s reputation: {row.Reputation}")
                .WithColor(colour);

            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("Leaderboard", "Top")]
        [DefaultCooldown(1, 5)]
        public async Task Leaderboard()
        {
            List<ReputationUserRow> rows = await Database.Data.Reputation.GetUserRowsAsync(Context.Guild.Id);
            rows = rows.OrderBy(x => x.Reputation).ToList();

            int position = 1;
            string content = "";

            foreach (ReputationUserRow row in rows)
            {
                IMember member = await Context.Guild.FetchMemberAsync(row.UserId);
                if (member is not null)
                {
                    content += $"{position}. {member.Mention} {row.Reputation}\n";
                    if (position == 10) break;
                    position++;
                }
            }

            await Context.Channel.SendInfoAsync("Reputation Leaderboard", content);
        }

        [Command("InverseLeaderboard", "Bottom")]
        [DefaultCooldown(1, 5)]
        public async Task InvserseLeaderboard()
        {
            List<ReputationUserRow> rows = await Database.Data.Reputation.GetUserRowsAsync(Context.Guild.Id);
            rows = rows.OrderBy(x => x.Reputation).ToList();

            int position = rows.Count;
            string content = "";

            foreach (ReputationUserRow row in rows)
            {
                IMember member = await Context.Guild.FetchMemberAsync(row.UserId);
                if (member is not null)
                {
                    content += $"{position}. {member.Mention} {row.Reputation}\n";
                    if (position == rows.Count - 10) break;
                    position--;
                }
            }

            await Context.Channel.SendInfoAsync("Inverse Reputation Leaderboard", content);
        }

        [Command("Give", "Add")]
        [DefaultCooldown(1, 2)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task Give(
            [RequireNotBot] 
            IMember member, 
            ulong change)
        {
            await Database.Data.Reputation.AlterUserReputationAsync(Context.Guild.Id, member.Id, (long)change);
            await Context.Channel.SendSuccessAsync("Reputation given", $"Gave {change} reputation to {member.Mention}");
        }

        [Command("Revoke", "Take")]
        [DefaultCooldown(1, 2)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task Take(
            [RequireNotBot] 
            IMember member, 
            ulong change)
        {
            await Database.Data.Reputation.AlterUserReputationAsync(Context.Guild.Id, member.Id, -(long)change);
            await Context.Channel.SendSuccessAsync("Reputation given", $"Took {change} reputation from {member.Mention}");
        }

        [Command("Set")]
        [DefaultCooldown(1, 2)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task Give(
            [RequireNotBot] 
            IMember member, 
            long amount)
        {
            await Database.Data.Reputation.SetUserReputationAsync(Context.Guild.Id, member.Id, amount);
            await Context.Channel.SendSuccessAsync("Reputation set", $"Set {member.Mention}'s reputation to {amount}");
        }

        [Command("AddEmoji")]
        [DefaultCooldown(2, 5)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        [RequireBotChannelPermissions(Permission.AddReactions)]
        public async Task AddEmoji(IEmoji emoji, int value = 0)
        {
            ReputationRow row = await Database.Data.Reputation.GetRowAsync(Context.Guild.Id);

            if (row.Emotes.Any(x => Equals(x.Item1, emoji.ToString())))
            {
                await Context.Channel.SendFailureAsync("Error", "That emoji is already added");
                return;
            }

            row.Emotes.Add((emoji.ToString(), value));
            await Database.Data.Reputation.SaveRowAsync(row);

            await Context.Channel.SendSuccessAsync("Emoji added",
                $"The {emoji} emoji was added successfully with value {value}\nYou can change its value or remove it on the dashboard");
        }
    }
}
