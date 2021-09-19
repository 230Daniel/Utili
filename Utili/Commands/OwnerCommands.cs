using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Database;
using Utili.Extensions;
using Qmmands;

namespace Utili.Commands
{
    public class OwnerCommands : DiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;
        
        public OwnerCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Command("UserInfo"), RequireBotOwner]
        public async Task UserInfo(ulong userId)
        {
            var user = Context.Bot.GetUser(userId) ?? await Context.Bot.FetchUserAsync(userId);
            
            var userRow = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            var subscriptions = await _dbContext.Subscriptions.Where(x => x.UserId == userId).ToListAsync();
            var customerDetails = await _dbContext.CustomerDetails.FirstOrDefaultAsync(x => x.UserId == userId);
            
            var content = $"Id: {user?.Id}\n" +
                          $"Email: {userRow.Email}\n" +
                          $"Customer: {customerDetails.CustomerId}\n" +
                          $"Subscriptions: {subscriptions.Count}\n" +
                          $"Premium slots: {subscriptions.Sum(x => x.Slots)}";

            var embed = Utils.MessageUtils.CreateEmbed(Utils.EmbedType.Info, user?.ToString(), content);
            embed.WithThumbnailUrl(user.GetAvatarUrl());

            await Context.Author.SendMessageAsync(new LocalMessage().AddEmbed(embed));
            await Context.Channel.SendSuccessAsync("User info sent",
                $"Information about {user} was sent in a direct message");
        }

        [Command("GuildInfo"), RequireBotOwner]
        public async Task GuildInfo(ulong guildId)
        {
            var guild = await Context.Bot.FetchGuildAsync(guildId, true);
            var premium = await _dbContext.PremiumSlots.AnyAsync(x => x.GuildId == guildId);

            var content = $"Id: {guild?.Id}\n" +
                          $"Owner: {guild.OwnerId}\n" +
                          $"Created: {guild.CreatedAt().UtcDateTime} UTC\n" +
                          $"Premium: {premium.ToString().ToLower()}";

            var embed = Utils.MessageUtils.CreateEmbed(Utils.EmbedType.Info, guild.ToString(), content);
            embed.WithThumbnailUrl(guild.GetIconUrl());

            await Context.Author.SendMessageAsync(new LocalMessage().AddEmbed(embed));
            await Context.Channel.SendSuccessAsync("Guild info sent",
                $"Information about {guild} was sent in a direct message");
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
                if (guild is null) await Context.Channel.SendFailureAsync("Error", "I'm not in that server", false);
                else if (member is null)
                    await Context.Channel.SendFailureAsync("Not authorised", $"The user is not a member of {guild}",
                        false);
                return;
            }

            var roles = member.RoleIds.Select(x => guild.Roles.First(y => y.Key == x).Value);
            var perms = Discord.Permissions.CalculatePermissions(guild, member, roles);

            if (guild.OwnerId == userId)
                await Context.Channel.SendSuccessAsync("Authorised", $"{member} is the owner of {guild}");
            else if (perms.Administrator)
                await Context.Channel.SendSuccessAsync("Authorised", $"{member} an administrator of {guild}");
            else if (perms.ManageGuild)
                await Context.Channel.SendSuccessAsync("Authorised",
                    $"{member} has the manage server permission in {guild}");
            else
                await Context.Channel.SendFailureAsync("Not authorised",
                    $"{member} does not have the manage server permission in {guild}", false);
        }
    }
}
