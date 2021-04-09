using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Utili.Extensions;
using static Utili.Program;
using Qmmands;

namespace Utili.Commands
{
    public class OwnerCommands : DiscordGuildModuleBase
    {
        [Command("UserInfo"), RequireBotOwner]
        public async Task UserInfo(ulong userId)
        {
            UserRow row = await Users.GetRowAsync(userId);
            IUser user = await Context.Bot.FetchUserAsync(userId);

            List<SubscriptionsRow> subscriptions = await Subscriptions.GetRowsAsync(userId: userId);

            string content = $"Id: {user?.Id}\n" +
                             $"Email: {row.Email}\n" +
                             $"Customer: {row.CustomerId}\n" +
                             $"Subscriptions: {subscriptions.Count}\n" +
                             $"Premium slots: {subscriptions.Sum(x => x.Slots)}";

            LocalEmbedBuilder embed = Utils.MessageUtils.CreateEmbed(Utils.EmbedType.Info, user?.ToString(), content);
            embed.WithThumbnailUrl(user.GetAvatarUrl());

            await Context.Author.SendMessageAsync(new LocalMessageBuilder().WithEmbed(embed).Build());
            await Context.Channel.SendSuccessAsync("User info sent", $"Information about {user} was sent in a direct message");
        }

        [Command("GuildInfo"), RequireBotOwner]
        public async Task GuildInfo(ulong guildId)
        {
            RestGuild guild = await _oldRest.GetGuildAsync(guildId, true);
            bool premium = await Premium.IsGuildPremiumAsync(guildId);

            string content = $"Id: {guild?.Id}\n" +
                             $"Owner: {guild.OwnerId}\n" +
                             $"Members: {guild.ApproximateMemberCount}\n" +
                             $"Created: {guild.CreatedAt.UtcDateTime} UTC\n" +
                             $"Premium: {premium.ToString().ToLower()}";

            LocalEmbedBuilder embed = Utils.MessageUtils.CreateEmbed(Utils.EmbedType.Info, guild.ToString(), content);
            embed.WithThumbnailUrl(guild.IconUrl);

            await Context.Author.SendMessageAsync(new LocalMessageBuilder().WithEmbed(embed).Build());
            await Context.Channel.SendSuccessAsync("Guild info sent", $"Information about {guild} was sent in a direct message");
        }

        [Command("Authorise"), RequireBotOwner]
        public async Task Authorise(ulong guildId, ulong userId)
        {
            IGuild guild = null;
            IMember member = null;
            try
            {
                guild = await Context.Bot.FetchGuildAsync(guildId);
                member = await guild.FetchMemberAsync(userId);
            }
            catch
            {
                if(guild is null) await Context.Channel.SendFailureAsync("Error", "I'm not in that server", false);
                else if(member is null) await Context.Channel.SendFailureAsync("Not authorised", $"The user is not a member of {guild}", false);
                return;
            }

            IEnumerable<IRole> roles = member.RoleIds.Select(x => guild.Roles.First(y => y.Key == x).Value);
            GuildPermissions perms = Disqord.Discord.Permissions.CalculatePermissions(guild, member, roles);

            if (guild.OwnerId == userId) await Context.Channel.SendSuccessAsync("Authorised", $"{member} is the owner of {guild}");
            else if (perms.Administrator) await Context.Channel.SendSuccessAsync("Authorised", $"{member} an administrator of {guild}");
            else if (perms.ManageGuild) await Context.Channel.SendSuccessAsync("Authorised", $"{member} has the manage server permission in {guild}");
            else await Context.Channel.SendFailureAsync("Not authorised", $"{member} does not have the manage server permission in {guild}", false);
        }

        [Command("Restart"), RequireBotOwner]
        public async Task Restart()
        {
            await Context.Channel.SendSuccessAsync("Restarting");
            _logger.Log("Command", $"Restart Requested by {Context.Author} - Killing process 10 seconds.", LogSeverity.Crit);
            await Task.Delay(10000);
            Monitoring.Restart();
        }
    }
}
