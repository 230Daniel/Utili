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
            return row.Mode switch
            {
                // All
                0 => true,

                // Images
                1 => _messageFilter.IsImage(context),

                // Videos
                2 => _messageFilter.IsVideo(context),

                // Media
                3 => _messageFilter.IsImage(context) || _messageFilter.IsVideo(context),

                // Music
                4 => _messageFilter.IsMusic(context) || _messageFilter.IsVideo(context),

                // Attachments
                5 => _messageFilter.IsAttachment(context),

                // URLs
                6 => _messageFilter.IsUrl(context),

                // URLs or Media
                7 => _messageFilter.IsImage(context) || _messageFilter.IsVideo(context) || _messageFilter.IsUrl(context),

                // Default
                _ => true,
            };
        }
    }
}
