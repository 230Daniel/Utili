using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
using NewDatabase;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using Qmmands;
using Utili.Extensions;
using Utili.Implementations;
using Utili.Utils;

namespace Utili.Commands
{
    [Group("Reputation", "Rep")]
    public class RepuatationCommands : DiscordInteractiveGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;
        
        public RepuatationCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Command("")]
        public async Task Reputation(
            [RequireNotBot]
            IMember member = null)
        {
            member ??= Context.Message.Author as IMember;

            var repMember = await _dbContext.ReputationMembers.GetForMemberAsync(Context.GuildId, member.Id);
            var reputation = repMember?.Reputation ?? 0;
            
            var colour = reputation switch
            {
                0 => new Color(195, 195, 195),
                > 0 => new Color(67, 181, 129),
                < 0 => new Color(181, 67, 67)
            };

            var embed = MessageUtils
                .CreateEmbed(EmbedType.Info, "", $"{member.Mention}'s reputation: {reputation}")
                .WithColor(colour);

            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("Leaderboard", "Top")]
        [DefaultCooldown(1, 5)]
        public async Task Leaderboard()
        {
            var repMembers = await _dbContext.ReputationMembers.GetForAllGuildMembersAsync(Context.GuildId);
            repMembers = repMembers.OrderBy(x => x.Reputation).ToList();

            var position = 1;
            var content = "";

            foreach (var repMember in repMembers)
            {
                var member = Context.Guild.GetMember(repMember.MemberId) ?? await Context.Guild.FetchMemberAsync(repMember.MemberId);
                if (member is not null)
                {
                    content += $"{position}. {member.Mention} {repMember.Reputation}\n";
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
            var repMembers = await _dbContext.ReputationMembers.GetForAllGuildMembersAsync(Context.GuildId);
            repMembers = repMembers.OrderBy(x => x.Reputation).ToList();

            var position = repMembers.Count;
            var content = "";

            foreach (var repMember in repMembers)
            {
                var member = Context.Guild.GetMember(repMember.MemberId) ?? await Context.Guild.FetchMemberAsync(repMember.MemberId);
                if (member is not null)
                {
                    content += $"{position}. {member.Mention} {repMember.Reputation}\n";
                    if (position == repMembers.Count - 10) break;
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
            await _dbContext.ReputationMembers.UpdateMemberReputationAsync(Context.GuildId, member.Id, (long)change);
            await _dbContext.SaveChangesAsync();
            
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
            await _dbContext.ReputationMembers.UpdateMemberReputationAsync(Context.GuildId, member.Id, -(long)change);
            await _dbContext.SaveChangesAsync();
            
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
            var repMember = await _dbContext.ReputationMembers.GetForMemberAsync(Context.GuildId, member.Id);
            
            if (repMember is null)
            {
                repMember = new ReputationMember(Context.GuildId, member.Id)
                {
                    Reputation = amount
                };
                _dbContext.ReputationMembers.Add(repMember);
            }
            else
            {
                repMember.Reputation = amount;
                _dbContext.ReputationMembers.Update(repMember);
            }
            
            await _dbContext.SaveChangesAsync();
            
            await Context.Channel.SendSuccessAsync("Reputation set", $"Set {member.Mention}'s reputation to {amount}");
        }

        [Command("AddEmoji")]
        [DefaultCooldown(2, 5)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task AddEmoji(IEmoji emoji, int value = 0)
        {
            var config = await _dbContext.ReputationConfigurations.GetForGuildWithEmojisAsync(Context.GuildId);
            config.Emojis ??= new List<ReputationConfigurationEmoji>();
            
            if (config.Emojis.Any(x => Equals(x.Emoji, emoji.ToString())))
            {
                await Context.Channel.SendFailureAsync("Error", "That emoji is already added");
                return;
            }

            config.Emojis.Add(new ReputationConfigurationEmoji(emoji.ToString())
            {
                Value = value
            });
            _dbContext.ReputationConfigurations.Update(config);
            await _dbContext.SaveChangesAsync();

            await Context.Channel.SendSuccessAsync("Emoji added",
                $"The {emoji} emoji was added successfully with value {value}\nYou can change its value or remove it on the dashboard");
        }

        [Command("Reset")]
        [DefaultCooldown(1, 10)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task Reset()
        {
            if (await ConfirmAsync("Are you sure?", "This command will reset reputation for all server members", "Reset all reputation"))
            {
                _dbContext.ReputationMembers.RemoveRange(await _dbContext.ReputationMembers.GetForAllGuildMembersAsync(Context.GuildId));
                await _dbContext.SaveChangesAsync();
                
                await Context.Channel.SendSuccessAsync("Reputation reset", "The reputation of all server members has been set to 0");
            }
            else
            {
                await Context.Channel.SendFailureAsync("Action canceled", "No action was performed");
            }
        }
    }
}
