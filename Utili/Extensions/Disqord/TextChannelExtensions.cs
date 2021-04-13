using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Utili.Services;
using Utili.Utils;

namespace Utili.Extensions
{
    static class TextChannelExtensions
    {
        public static async Task<IUserMessage> SendInfoAsync(this ITextChannel channel, string title, string content = null)
        {
            LocalMessage message = new LocalMessageBuilder()
                .WithEmbed(MessageUtils.CreateEmbed(EmbedType.Info, title, content))
                .Build();

            return await channel.SendMessageAsync(message);
        }

        public static async Task<IUserMessage> SendSuccessAsync(this ITextChannel channel, string title, string content = null)
        {
            LocalMessage message = new LocalMessageBuilder()
                .WithEmbed(MessageUtils.CreateEmbed(EmbedType.Success, title, content))
                .Build();

            return await channel.SendMessageAsync(message);
        }

        public static async Task<IUserMessage> SendFailureAsync(this ITextChannel channel, string title, string content = null, bool supportLink = true)
        {
            LocalMessage message = new LocalMessageBuilder()
                .WithEmbed(MessageUtils.CreateEmbed(EmbedType.Failure, title, content))
                .Build();

            return await channel.SendMessageAsync(message);
        }

        public static async Task<IUserMessage> SendEmbedAsync(this ITextChannel channel, LocalEmbedBuilder embed)
        {
            LocalMessage message = new LocalMessageBuilder()
                .WithEmbed(embed)
                .Build();

            return await channel.SendMessageAsync(message);
        }

        public static bool BotHasPermissions(this CachedTextChannel channel, DiscordClientBase client, params Permission[] requiredPermissions)
        {
            CachedGuild guild = client.GetGuild(channel.GuildId);
            IMember bot = guild.Members.GetValueOrDefault(client.CurrentUser.Id);
            List<CachedRole> roles = bot.GetRoles().Values.ToList();

            ChannelPermissions permissions = Disqord.Discord.Permissions.CalculatePermissions(guild, channel, bot, roles);
            return requiredPermissions.All(x => permissions.Contains(x));
        }

        public static bool BotHasPermissions(this ITextChannel channel, DiscordClientBase client, out string missingPermissions, params Permission[] requiredPermissions)
        {
            CachedGuild guild = client.GetGuild(channel.GuildId);
            IMember bot = guild.Members.GetValueOrDefault(client.CurrentUser.Id);
            IEnumerable<CachedRole> roles = bot.GetRoles().Values;

            ChannelPermissions permissions = Disqord.Discord.Permissions.CalculatePermissions(guild, channel, bot, roles);

            missingPermissions = "";

            return requiredPermissions.All(x => permissions.Contains(x));
        }
    }
}
