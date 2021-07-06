using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Qmmands;
using Utili.Extensions;
using Utili.Implementations;
using Utili.Utils;

namespace Utili.Commands
{
    [Group("Inactive", "InactiveRole")]
    public class InactiveRoleCommands : DiscordInteractiveGuildModuleBase
    {
        private static List<ulong> _kickingIn = new();

        [Command("List")]
        [Cooldown(1, 10, CooldownMeasure.Seconds, CooldownBucketType.Guild)]
        public async Task List()
        {
            var row = await InactiveRole.GetRowAsync(Context.Guild.Id);
            if (Context.Guild.GetRole(row.RoleId) is null)
            {
                await Context.Channel.SendFailureAsync("Error", "This server does not have an inactive role set");
                return; 
            }

            var members = await Context.Guild.FetchAllMembersAsync();
            var inactiveMembers = members
                .Where(x => x.GetRole(row.RoleId) is not null && x.GetRole(row.ImmuneRoleId) is null)
                .OrderBy(x => x.Nick ?? x.Name)
                .ToList();

            if (inactiveMembers.Count == 0)
            {
                await Context.Channel.SendInfoAsync("Inactive Users", "None");
                return;
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
            await View(menu);
        }

        [Command("Kick")]
        [RequireAuthorGuildPermissions(Permission.Administrator)]
        [RequireBotGuildPermissions(Permission.KickMembers)]
        [RequireBotChannelPermissions(Permission.AddReactions)]
        [Cooldown(1, 10, CooldownMeasure.Seconds, CooldownBucketType.Guild)]
        public async Task Kick()
        {
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

            var row = await InactiveRole.GetRowAsync(Context.Guild.Id);
            if (Context.Guild.GetRole(row.RoleId) is null)
            {
                await Context.Channel.SendFailureAsync("Error", "This server does not have an inactive role set");
                return; 
            }

            var members = await Context.Guild.FetchAllMembersAsync();
            var inactiveMembers = members
                .Where(x => x.GetRole(row.RoleId) is not null && x.GetRole(row.ImmuneRoleId) is null)
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
