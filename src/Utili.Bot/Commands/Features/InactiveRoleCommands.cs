using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Utili.Database;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Qmmands;
using Qmmands.Text;
using Utili.Bot.Implementations;
using Utili.Bot.Implementations.Views;
using Utili.Bot.Services;
using Utili.Bot.Utils;
using Utili.Bot.Extensions;

namespace Utili.Bot.Commands;

[TextGroup("inactive", "inactiverole")]
public class InactiveRoleCommands : MyDiscordTextGuildModuleBase
{
    private readonly DatabaseContext _dbContext;
    private readonly MemberCacheService _memberCache;
    private static List<ulong> _kickingIn = new();

    public InactiveRoleCommands(DatabaseContext dbContext, MemberCacheService memberCache)
    {
        _dbContext = dbContext;
        _memberCache = memberCache;
    }

    [TextCommand("list")]
    public async Task<IResult> ListAsync()
    {
        var config = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(Context.GuildId);
        if (config is null || Context.GetGuild().GetRole(config.RoleId) is null)
            return Failure("Error", "This server does not have an inactive role set");

        await _memberCache.TemporarilyCacheMembersAsync(Context.GuildId);
        var inactiveMembers = config.Mode == InactiveRoleMode.GrantWhenInactive
            ? Context.GetGuild().GetMembers().Values
                .Where(x => x.GetRole(config.RoleId) is not null && x.GetRole(config.ImmuneRoleId) is null)
                .OrderBy(x => x.Nick ?? x.Name)
                .ToList()
            : Context.GetGuild().GetMembers().Values
                .Where(x => x.GetRole(config.RoleId) is null && x.GetRole(config.ImmuneRoleId) is null)
                .OrderBy(x => x.Nick ?? x.Name)
                .ToList();

        if (inactiveMembers.Count == 0)
            return Info("Inactive Users", "None");

        var pages = new List<Page>();
        var content = "";
        var embed = MessageUtils.CreateEmbed(EmbedType.Info, "Inactive Users");

        for (var i = 0; i < inactiveMembers.Count; i++)
        {
            content += $"{inactiveMembers[i].Mention}\n";
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
                embed = MessageUtils.CreateEmbed(EmbedType.Info, "Inactive Members");
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

    [TextCommand("kick")]
    [RequireAuthorPermissions(Permissions.Administrator)]
    [RequireBotPermissions(Permissions.KickMembers | Permissions.AddReactions)]
    [RateLimit(1, 10, RateLimitMeasure.Seconds, RateLimitBucketType.Guild)]
    public async Task<IResult> KickAsync()
    {
        var config = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(Context.GuildId);
        if (config is null || Context.GetGuild().GetRole(config.RoleId) is null)
            return Failure("Error", "This server does not have an inactive role set");

        lock (_kickingIn)
        {
            if (_kickingIn.Contains(Context.GuildId))
                return Failure("Error", "This command is already being executed in this server.");

            _kickingIn.Add(Context.GuildId);
        }

        try
        {
            await _memberCache.TemporarilyCacheMembersAsync(Context.GuildId);
            var inactiveMembers = config.Mode == InactiveRoleMode.GrantWhenInactive
                ? Context.GetGuild().GetMembers().Values
                    .Where(x => x.GetRole(config.RoleId) is not null && x.GetRole(config.ImmuneRoleId) is null)
                    .OrderBy(x => x.Nick ?? x.Name)
                    .ToList()
                : Context.GetGuild().GetMembers().Values
                    .Where(x => x.GetRole(config.RoleId) is null && x.GetRole(config.ImmuneRoleId) is null)
                    .OrderBy(x => x.Nick ?? x.Name)
                    .ToList();

            if (await ConfirmAsync(new ConfirmViewOptions
                {
                    PromptDescription = $"This command will kick {inactiveMembers.Count} inactive members - View them with {Context.Prefix}inactive list",
                    PromptConfirmButtonLabel = $"Kick {inactiveMembers.Count} inactive members",
                    ConfirmTitle = $"Kicking {inactiveMembers.Count} inactive members",
                    ConfirmDescription = $"Under ideal conditions, this action will take {TimeSpan.FromSeconds(inactiveMembers.Count * 1.1).ToLongString()}"
                }))
            {
                var failed = 0;
                foreach (var member in inactiveMembers)
                {
                    try
                    {
                        var delay = Task.Delay(1100);
                        var kick = member.KickAsync(new DefaultRestRequestOptions { Reason = $"Inactive Kick (manual by {Context.Message.Author} {Context.Message.Author.Id})" });
                        await Task.WhenAll(delay, kick);
                    }
                    catch
                    {
                        failed++;
                    }
                }

                return Success(
                    "Inactive members kicked",
                    $"{inactiveMembers.Count - failed} inactive members were kicked {(failed > 0 ? $"\nFailed to kick {failed} members" : "")}");
            }

            return null;
        }
        finally
        {
            lock (_kickingIn)
            {
                _kickingIn.Remove(Context.GuildId);
            }
        }
    }
}
