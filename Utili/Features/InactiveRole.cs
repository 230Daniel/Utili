using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Utili.Commands;
using Utili.Handlers;
using static Utili.Program;
using static Utili.MessageSender;

namespace Utili.Features
{
    internal static class InactiveRole
    {
        private static Timer _timer;

        public static async Task UpdateUserAsync(SocketGuild guild, SocketGuildUser user)
        {
            InactiveRoleRow row = await Database.Data.InactiveRole.GetRowAsync(guild.Id);

            if(guild.Roles.Any(x => x.Id == row.RoleId))
            {
                await Database.Data.InactiveRole.UpdateUserAsync(guild.Id, user.Id);

                if (user.Roles.Any(x => x.Id == row.RoleId))
                {
                    await user.RemoveRoleAsync(guild.GetRole(row.RoleId));
                }
            }
        }

        public static void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(30000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                UpdateGuildsAsync().GetAwaiter().GetResult();
            }
            catch(Exception er)
            {
                _logger.ReportError("InactiveRole", er);
            }
        }

        private static async Task UpdateGuildsAsync()
        {
            List<InactiveRoleRow> guildsRequiringUpdate = (await Database.Data.InactiveRole.GetUpdateRequiredRowsAsync()).Where(x => _client.Guilds.Any(y => y.Id == x.GuildId)).Take(5).ToList();

            foreach (InactiveRoleRow row in guildsRequiringUpdate)
            {
                row.LastUpdate = DateTime.UtcNow;
                await Database.Data.InactiveRole.SaveLastUpdateAsync(row);
            }

            List<Task> tasks = new List<Task>();

            foreach (InactiveRoleRow row in guildsRequiringUpdate.Where(x => _client.Guilds.Any(y => y.Id == x.GuildId)))
            {
                SocketGuild guild = _client.GetGuild(row.GuildId);
                tasks.Add(UpdateGuildAsync(row, guild));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task UpdateGuildAsync(InactiveRoleRow row, SocketGuild guild)
        {
            SocketRole inactiveRole = guild.GetRole(row.RoleId);
            if(inactiveRole is null) return;

            if (!BotPermissions.CanManageRole(inactiveRole)) return;

            bool premium = await Premium.IsGuildPremiumAsync(guild.Id);

            List<IGuildUser> users = (await guild.GetUsersAsync().FlattenAsync()).ToList();
            IGuildUser bot = users.First(x => x.Id == _client.CurrentUser.Id);
            List<InactiveRoleUserRow> userRows = await Database.Data.InactiveRole.GetUsersAsync(guild.Id);

            foreach (IGuildUser user in users.Where(x => !x.IsBot).OrderBy(x => x.Id))
            {
                // DefaultLastAction is set to the time when the activity data started being recorded
                DateTime lastAction = row.DefaultLastAction;

                if (bot.JoinedAt.HasValue && bot.JoinedAt > lastAction) 
                    lastAction = bot.JoinedAt.Value.UtcDateTime;

                List<InactiveRoleUserRow> matchingRows = userRows.Where(x => x.UserId == user.Id).ToList();
                if (matchingRows.Count > 0 && matchingRows.First().LastAction > lastAction)
                    lastAction = matchingRows.First().LastAction;

                if (user.JoinedAt.HasValue && user.JoinedAt.Value.UtcDateTime > lastAction)
                    lastAction = user.JoinedAt.Value.UtcDateTime;

                DateTime minimumLastAction = DateTime.UtcNow - row.Threshold;
                DateTime minimumKickLastAction = DateTime.UtcNow - (row.Threshold + row.AutoKickThreshold);

                if (lastAction <= minimumLastAction && !user.RoleIds.Contains(row.ImmuneRoleId))
                {
                    if (premium && row.AutoKick && lastAction <= minimumKickLastAction)
                    {
                        if (guild.BotHasPermissions(GuildPermission.KickMembers))
                            await user.KickAsync();
                        await Task.Delay(500);
                        continue;
                    }

                    if (!row.Inverse)
                    {
                        if (!user.RoleIds.Contains(inactiveRole.Id))
                        {
                            await user.AddRoleAsync(inactiveRole);
                            await Task.Delay(500);
                        }
                    }
                    else
                    {
                        if (user.RoleIds.Contains(inactiveRole.Id))
                        {
                            await user.RemoveRoleAsync(inactiveRole);
                            await Task.Delay(500);
                        }
                    }
                }
                else
                {
                    if (!row.Inverse)
                    {
                        if (user.RoleIds.Contains(inactiveRole.Id))
                        {
                            await user.RemoveRoleAsync(inactiveRole);
                            await Task.Delay(500);
                        }
                    }
                    else
                    {
                        if (!user.RoleIds.Contains(inactiveRole.Id))
                        {
                            await user.AddRoleAsync(inactiveRole);
                            await Task.Delay(500);
                        }
                    }
                }
            }
        }
    }

    [Group("Inactive"), Alias("InactiveRole")]
    public class InactiveRoleCommands : ModuleBase<SocketCommandContext>
    {
        [Command("List")]
        public async Task List(int page = 1)
        {
            InactiveRoleRow row = await Database.Data.InactiveRole.GetRowAsync(Context.Guild.Id);
            if (Context.Guild.Roles.All(x => x.Id != row.RoleId))
            {
                await SendFailureAsync(Context.Channel, "Error", "This server does not have an inactive role set");
                return; 
            }

            await Context.Guild.DownloadAndKeepUsersAsync(TimeSpan.FromMinutes(15));

            List<SocketGuildUser> users = Context.Guild.Users.Where(x => x.Roles.Any(y => y.Id == row.RoleId)).ToList();
            users = users.Where(x => x.Roles.All(y => y.Id != row.ImmuneRoleId)).ToList();
            users = users.OrderBy(x => x.Nickname ?? x.Username).ToList();

            int totalPages = (int) Math.Ceiling(users.Count / 50d);
            users = users.Skip((page - 1) * 50).Take(50).ToList();
            if ((page < 1 || page > totalPages) && totalPages != 0)
            {
                await SendFailureAsync(Context.Channel, "Error", "Invalid page number");
                return; 
            }
            if (totalPages == 0) page = 0;

            string output = users.Aggregate("", (current, user) => current + $"{user.Mention}\n");
            if (output == "") output = "There are no inactive users.";

            await SendInfoAsync(Context.Channel, "Inactive Users", output, $"Page {page} of {totalPages}");
        }

        private static List<ulong> _kickingIn = new List<ulong>();

        [Command("Kick"), Cooldown(2), Permission(Perm.ManageGuild)]
        public async Task Kick(string confirm = "")
        {
            if (_kickingIn.Contains(Context.Guild.Id))
            {
                await SendFailureAsync(Context.Channel, "Error", "A large operation is already taking place in this server, please wait before it has completed before starting another.");
                return; 
            }

            if (BotPermissions.IsMissingPermissions(Context.Guild, new[] {GuildPermission.KickMembers}, out string missingPermissions))
            {
                await SendFailureAsync(Context.Channel, "Error", $"I'm missing the following permissions: {missingPermissions}");
                return;
            }

            InactiveRoleRow row = await Database.Data.InactiveRole.GetRowAsync(Context.Guild.Id);
            if (Context.Guild.Roles.All(x => x.Id != row.RoleId))
            {
                await SendFailureAsync(Context.Channel, "Error", "This server does not have an inactive role set");
                return; 
            }

            _kickingIn.Add(Context.Guild.Id);
            await Context.Guild.DownloadAndKeepUsersAsync(TimeSpan.FromMinutes(15));

            List<SocketGuildUser> users = Context.Guild.Users.Where(x => x.Roles.Any(y => y.Id == row.RoleId)).ToList();
            users = users.Where(x => x.Roles.All(y => y.Id != row.ImmuneRoleId)).ToList();
            users = users.OrderBy(x => x.Nickname ?? x.Username).ToList();

            if (confirm.ToLower() != "confirm")
            {
                await SendInfoAsync(Context.Channel, "Are you sure?", $"This operation will kick **{users.Count}** inactive user{(users.Count == 1 ? "" : "s")}.\nUse `inactive kick confirm` to kick these users now.");

                _kickingIn.Remove(Context.Guild.Id);
                return;
            }

            await SendSuccessAsync(Context.Channel, $"Kicking {users.Count} inactive users", $"This operation will take {TimeSpan.FromSeconds(users.Count * 1.2).ToLongString()}.");

            foreach (SocketGuildUser user in users)
            {
                _ = user.KickAsync();
                await Task.Delay(1200);
            }

            await SendSuccessAsync(Context.Channel, $"Kicked {users.Count} inactive users", "The operation ran successfully.");

            _kickingIn.Remove(Context.Guild.Id);
        }
    }
}
