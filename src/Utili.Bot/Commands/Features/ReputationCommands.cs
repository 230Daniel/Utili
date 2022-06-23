using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
using Utili.Database;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using Utili.Bot.Implementations;
using Utili.Bot.Implementations.Views;
using Utili.Bot.Utils;
using Utili.Bot.Extensions;

namespace Utili.Bot.Commands
{
    [Group("reputation", "rep")]
    public class RepuatationCommands : MyDiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;

        public RepuatationCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Command("")]
        public async Task<DiscordCommandResult> ReputationAsync(
            [RequireNotBot] IMember member = null)
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

            return Response(embed);
        }

        [Command("leaderboard", "top")]
        [DefaultCooldown(1, 5)]
        public async Task<DiscordCommandResult> LeaderboardAsync()
        {
            var repMembers = await _dbContext.ReputationMembers.GetForAllGuildMembersAsync(Context.GuildId);
            repMembers = repMembers.OrderByDescending(x => x.Reputation).ToList();

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

            return Info("Reputation Leaderboard", content);
        }

        [Command("inverseleaderboard", "bottom")]
        [DefaultCooldown(1, 5)]
        public async Task<DiscordCommandResult> InvserseLeaderboardAsync()
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

            return Info("Inverse Reputation Leaderboard", content);
        }

        [Command("give", "add", "grant")]
        [DefaultCooldown(1, 2)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> GiveAsync(
            [RequireNotBot] IMember member,
            ulong change)
        {
            await _dbContext.ReputationMembers.UpdateMemberReputationAsync(Context.GuildId, member.Id, (long)change);
            await _dbContext.SaveChangesAsync();

            return Success("Reputation given", $"Gave {change} reputation to {member.Mention}");
        }

        [Command("take", "revoke")]
        [DefaultCooldown(1, 2)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> TakeAsync(
            [RequireNotBot] IMember member,
            ulong change)
        {
            await _dbContext.ReputationMembers.UpdateMemberReputationAsync(Context.GuildId, member.Id, -(long)change);
            await _dbContext.SaveChangesAsync();

            return Success("Reputation taken", $"Took {change} reputation from {member.Mention}");
        }

        [Command("set")]
        [DefaultCooldown(1, 2)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> SetAsync(
            [RequireNotBot] IMember member,
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

            return Success("Reputation set", $"Set {member.Mention}'s reputation to {amount}");
        }

        [Command("addemoji")]
        [DefaultCooldown(2, 5)]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> AddEmojiAsync(IEmoji emoji, int value = 0)
        {
            var config = await _dbContext.ReputationConfigurations.GetForGuildWithEmojisAsync(Context.GuildId);
            if (config is null)
            {
                config = new ReputationConfiguration(Context.GuildId)
                {
                    Emojis = new List<ReputationConfigurationEmoji>
                    {
                        new(emoji.ToString())
                        {
                            Value = value
                        }
                    }
                };

                _dbContext.ReputationConfigurations.Add(config);
                await _dbContext.SetHasFeatureAsync(Context.GuildId, BotFeatures.Reputation, true);
            }
            else
            {
                config.Emojis ??= new List<ReputationConfigurationEmoji>();

                if (config.Emojis.Any(x => Equals(x.Emoji, emoji.ToString())))
                    return Failure("Error", "That emoji is already added");

                config.Emojis.Add(new ReputationConfigurationEmoji(emoji.ToString())
                {
                    Value = value
                });

                _dbContext.ReputationConfigurations.Update(config);
            }

            await _dbContext.SaveChangesAsync();
            return Success("Emoji added",
                $"The {emoji} emoji was added successfully with value {value}\nYou can change its value or remove it on the dashboard");
        }

        [Command("reset")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> ResetAsync()
        {
            if (await ConfirmAsync(new ConfirmViewOptions
                {
                    PromptDescription = "This command will reset reputation for all server members",
                    PromptConfirmButtonLabel = "Reset all reputation",
                    ConfirmTitle = "Reputation reset",
                    ConfirmDescription = "The reputation of all server members has been set to 0"
                }))
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM reputation_members WHERE guild_id = {Context.GuildId.RawValue};");
            }

            return null;
        }
    }
}
