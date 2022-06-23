using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Database;
using Database.Entities;
using Database.Extensions;
using Qmmands;
using Utili.Extensions;
using Utili.Implementations;
using Utili.Implementations.Views;
using Utili.Services;
using Utili.Utils;

namespace Utili.Commands
{
    [Group("inactive", "inactiverole")]
    public class InactiveRoleCommands : MyDiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly MemberCacheService _memberCache;
        private static List<ulong> _kickingIn = new();

        public InactiveRoleCommands(DatabaseContext dbContext, MemberCacheService memberCache)
        {
            _dbContext = dbContext;
            _memberCache = memberCache;
        }

        [Command("list")]
        public async Task<DiscordCommandResult> ListAsync()
        {
            var config = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(Context.GuildId);
            if (Context.Guild.GetRole(config.RoleId) is null)
                return Failure("Error", "This server does not have an inactive role set");

            await _memberCache.TemporarilyCacheMembersAsync(Context.GuildId);
            var inactiveMembers = config.Mode == InactiveRoleMode.GrantWhenInactive
                ? Context.Guild.GetMembers().Values
                    .Where(x => x.GetRole(config.RoleId) is not null && x.GetRole(config.ImmuneRoleId) is null)
                    .OrderBy(x => x.Nick ?? x.Name)
                    .ToList()
                : Context.Guild.GetMembers().Values
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
                        .WithIsInline(true));
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
                    .WithIsInline(true));

            if (embed.Fields.Count > 0)
                pages.Add(new Page().AddEmbed(embed));

            var pageProvider = new ListPageProvider(pages);
            var menu = new MyPagedView(pageProvider);
            return View(menu, TimeSpan.FromMinutes(5));
        }

        [Command("kick")]
        [RequireAuthorGuildPermissions(Permission.Administrator)]
        [RequireBotGuildPermissions(Permission.KickMembers)]
        [RequireBotChannelPermissions(Permission.AddReactions)]
        [Cooldown(1, 10, CooldownMeasure.Seconds, CooldownBucketType.Guild)]
        public async Task<DiscordCommandResult> KickAsync()
        {
            var config = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(Context.GuildId);
            if (Context.Guild.GetRole(config.RoleId) is null)
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
                    ? Context.Guild.GetMembers().Values
                        .Where(x => x.GetRole(config.RoleId) is not null && x.GetRole(config.ImmuneRoleId) is null)
                        .OrderBy(x => x.Nick ?? x.Name)
                        .ToList()
                    : Context.Guild.GetMembers().Values
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
                    await using var yield = Context.BeginYield();

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
}
