using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.Rest;
using static Utili.Program;
using static Utili.MessageSender;

namespace Utili.Commands
{
    public class OwnerCommands : ModuleBase<SocketCommandContext>
    {
        [Command("UserInfo")] [Permission(Perm.BotOwner)]
        public async Task UserInfo(ulong userId)
        {
            UserRow row = await Users.GetRowAsync(userId);
            RestUser user = await _rest.GetUserAsync(userId);
            List<SubscriptionsRow> subscriptions = await Subscriptions.GetRowsAsync(userId: userId);

            string content = $"Id: {user?.Id}\n" +
                             $"Email: {row.Email}\n" +
                             $"Customer: {row.CustomerId}\n" +
                             $"Subscriptions: {subscriptions.Count}\n" +
                             $"Premium slots: {subscriptions.Sum(x => x.Slots)}";

            EmbedBuilder embed = GenerateEmbed(
                MessageSender.EmbedType.Success,
                user?.ToString(),
                content).ToEmbedBuilder();

            embed.ThumbnailUrl = user?.GetAvatarUrl() ?? user?.GetDefaultAvatarUrl();

            await Context.User.SendMessageAsync(embed: embed.Build());

            await SendSuccessAsync(Context.Channel, "User info sent", $"Information about {user} was sent in a direct message");
        }

        [Command("Authorise")] [Permission(Perm.BotOwner)]
        public async Task Authorise(ulong guildId, ulong userId)
        {
            RestGuild guild = null;
            RestGuildUser user = null;
            try
            {
                guild = await _rest.GetGuildAsync(guildId);
                user = await guild.GetUserAsync(userId);
            }
            catch
            {
                if(guild is null) await SendFailureAsync(Context.Channel, "Error", "I'm not in that server", supportLink: false);
                else if(user is null) await SendFailureAsync(Context.Channel, "Not authorised", $"The user is not a member of {guild}", supportLink: false);
                return;
            }

            if (guild.OwnerId == userId) await SendSuccessAsync(Context.Channel, "Authorised", $"{user} is the owner of {guild}");
            else if (user.GuildPermissions.Administrator) await SendSuccessAsync(Context.Channel, "Authorised", $"{user} an administrator of {guild}");
            else if (user.GuildPermissions.ManageGuild) await SendSuccessAsync(Context.Channel, "Authorised", $"{user} has the manage server permission in {guild}");
            else await SendFailureAsync(Context.Channel, "Not authorised", $"{user} does not have the manage server permission in {guild}", supportLink: false);
        }
    }
}
