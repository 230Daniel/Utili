using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Utili.Database;
using Utili.Database.Extensions;
using Qmmands;
using Qmmands.Text;
using Utili.Bot.Implementations;
using Utili.Bot.Services;

namespace Utili.Bot.Commands;

public class OwnerCommands : MyDiscordTextGuildModuleBase
{
    private readonly DatabaseContext _dbContext;
    private readonly IsPremiumService _isPremiumService;

    public OwnerCommands(DatabaseContext dbContext, IsPremiumService isPremiumService)
    {
        _dbContext = dbContext;
        _isPremiumService = isPremiumService;
    }

    [TextCommand("userinfo"), RequireBotOwner]
    public async Task<IResult> UserInfoAsync(ulong userId)
    {
        var user = Context.Bot.GetUser(userId) as IUser ?? await Context.Bot.FetchUserAsync(userId);

        var userRow = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        var subscriptions = await _dbContext.Subscriptions.GetValidForUserAsync(userId);
        var customerDetails = await _dbContext.CustomerDetails.FirstOrDefaultAsync(x => x.UserId == userId);

        var content = $"Id: {user?.Id}\n" +
                      $"Email: {userRow.Email}\n" +
                      $"Customer: {customerDetails.CustomerId}\n" +
                      $"Valid subscriptions: {subscriptions.Count}\n" +
                      $"Premium slots: {subscriptions.Sum(x => x.Slots)}";

        var embed = Utils.MessageUtils.CreateEmbed(Utils.EmbedType.Info, user?.ToString(), content);
        embed.WithThumbnailUrl(user.GetAvatarUrl());

        await Context.Author.SendMessageAsync(new LocalMessage().AddEmbed(embed));
        return Success("User info sent",
            $"Information about {user} was sent in a direct message");
    }

    [TextCommand("guildinfo"), RequireBotOwner]
    public async Task<IResult> GuildInfoAsync(ulong guildId)
    {
        var guild = await Context.Bot.FetchGuildAsync(guildId, true);
        var premium = await _isPremiumService.GetIsGuildPremiumAsync(guildId);

        var content = $"Id: {guild?.Id}\n" +
                      $"Owner: {guild.OwnerId}\n" +
                      $"Created: {guild.CreatedAt().UtcDateTime} UTC\n" +
                      $"Premium: {premium.ToString().ToLower()}";

        var embed = Utils.MessageUtils.CreateEmbed(Utils.EmbedType.Info, guild.ToString(), content);
        embed.WithThumbnailUrl(guild.GetIconUrl());

        await Context.Author.SendMessageAsync(new LocalMessage().AddEmbed(embed));
        return Success("Guild info sent",
            $"Information about {guild} was sent in a direct message");
    }

    [TextCommand("authorise"), RequireBotOwner]
    public async Task<IResult> AuthoriseAsync(ulong guildId, ulong userId)
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
            if (guild is null) return Failure("Error", "I'm not in that server");
            if (member is null)
                return Failure("Not authorised", $"The user is not a member of {guild}");
        }

        var roles = member.RoleIds.Select(x => guild.Roles.First(y => y.Key == x).Value);
        var perms = Discord.PermissionCalculation.CalculateGuildPermissions(guild, member, roles.ToArray()); // todo: revert `roles.ToArray()` to `roles` when quah sorts his shit out

        if (guild.OwnerId == userId)
            return Success("Authorised", $"{member} is the owner of {guild}");
        if (perms.HasFlag(Permissions.Administrator))
            return Success("Authorised", $"{member} an administrator of {guild}");
        if (perms.HasFlag(Permissions.ManageGuild))
            return Success("Authorised", $"{member} has the manage server permission in {guild}");
        return Failure("Not authorised", $"{member} does not have the manage server permission in {guild}");
    }
}
