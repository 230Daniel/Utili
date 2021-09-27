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
using Utili.Services;
using Utili.Utils;

namespace Utili.Commands
{
    [Group("Inactive", "InactiveRole")]
    public class InactiveRoleCommands : DiscordInteractiveGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly MemberCacheService _memberCache;
        private static List<ulong> _kickingIn = new();

        public InactiveRoleCommands(DatabaseContext dbContext, MemberCacheService memberCache)
        {
            _dbContext = dbContext;
            _memberCache = memberCache;
        }
        
        [Command("List")]
        public async Task<DiscordCommandResult> List()
        {
            var config = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(Context.GuildId);
            if (Context.Guild.GetRole(config.RoleId) is null)
            {
                await Context.Channel.SendFailureAsync("Error", "This server does not have an inactive role set");
                return null; 
            }

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
            {
                await Context.Channel.SendInfoAsync("Inactive Users", "None");
                return null;
            }

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

        [Command("Kick")]
        [RequireAuthorGuildPermissions(Permission.Administrator)]
        [RequireBotGuildPermissions(Permission.KickMembers)]
        [RequireBotChannelPermissions(Permission.AddReactions)]
        [Cooldown(1, 10, CooldownMeasure.Seconds, CooldownBucketType.Guild)]
        public async Task Kick()
        {
            var config = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(Context.GuildId);
            if (Context.Guild.GetRole(config.RoleId) is null)
            {
                await Context.Channel.SendFailureAsync("Error", "This server does not have an inactive role set");
                return; 
            }

            var cancelCommand = false;
            
            lock (_kickingIn)
            {
                if (_kickingIn.Contains(Context.GuildId)) cancelCommand = true;
                else _kickingIn.Add(Context.GuildId);
            }

            if (cancelCommand)
            {
                await Context.Channel.SendFailureAsync("Error", "This command is already being executed in this server.");
                return;
            }
            
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

            if (await ConfirmAsync("Are you sure?", $"This command will kick {inactiveMembers.Count} inactive members - View them with {Context.Prefix}inactive list", $"Kick {inactiveMembers.Count} inactive members"))
            {
                await Context.Channel.SendSuccessAsync($"Kicking {inactiveMembers.Count} inactive members", $"Under ideal conditions, this action will take {TimeSpan.FromSeconds(inactiveMembers.Count * 1.1).ToLongString()}");
                
                var failed = 0;
                foreach(var member in inactiveMembers)
                {
                    try
                    {
                        var delay = Task.Delay(1100);
                        var kick = member.KickAsync(new DefaultRestRequestOptions {Reason = $"Inactive Kick (manual by {Context.Message.Author} {Context.Message.Author.Id})"});
                        await Task.WhenAll(delay, kick);
                    }
                    catch
                    {
                        failed++;
                    }
                }

                await Context.Channel.SendSuccessAsync("Inactive members kicked",
                    $"{inactiveMembers.Count - failed} inactive members were kicked {(failed > 0 ? $"\nFailed to kick {failed} members" : "")}");
            }
            else
            {
                await Context.Channel.SendFailureAsync("Action canceled", "No action was performed");
            }

            lock (_kickingIn)
            {
                _kickingIn.Remove(Context.GuildId);
            }
        }
    }
}
