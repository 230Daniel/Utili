using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord;
using Discord.Commands;
using static Utili.Program;

namespace Utili.Features
{
    internal class VoteChannels
    {
        public async Task MessageReceived(SocketCommandContext context)
        {
            if (BotPermissions.IsMissingPermissions(context.Channel, new[] {ChannelPermission.AddReactions}, out _))
            {
                return;
            }

            List<VoteChannelsRow> rows = Database.Data.VoteChannels.GetRows(context.Guild.Id, context.Channel.Id);

            if (rows.Count == 0)
            {
                return;
            }

            VoteChannelsRow row = rows.First();

            if (!DoesMessageObeyRule(context, row))
            {
                return;
            }

            List<IEmote> emotes = row.Emotes;

            if (Premium.IsPremium(context.Guild.Id))
            {
                emotes = emotes.Take(5).ToList();
            }
            else
            {
                emotes = emotes.Take(2).ToList();
            }

            await context.Message.AddReactionsAsync(emotes.ToArray());
        }

        public bool DoesMessageObeyRule(SocketCommandContext context, VoteChannelsRow row)
        {
            switch (row.Mode)
            {
                case 0: // All
                    return true;

                case 1: // Images
                    return _messageFilter.IsImage(context);

                case 2: // Videos
                    return _messageFilter.IsVideo(context);

                case 3: // Media
                    return _messageFilter.IsImage(context) || _messageFilter.IsVideo(context);

                case 4: // Music
                    return _messageFilter.IsMusic(context) || _messageFilter.IsVideo(context);

                case 5: // Attachments
                    return _messageFilter.IsAttachment(context);

                case 6: // URLs
                    return _messageFilter.IsUrl(context);

                case 7: // URLs or Media
                    return _messageFilter.IsImage(context) || _messageFilter.IsVideo(context) || _messageFilter.IsUrl(context);

                default:
                    return true;
            }
        }
    }
}
