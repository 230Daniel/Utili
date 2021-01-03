using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace DataTransfer.Transfer
{
    internal static class VoteChannels
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> channels;
            if (oneGuildId == null) channels = V1Data.GetDataList(type: "Votes-Channel");
            else channels = V1Data.GetDataList(oneGuildId.ToString(), "Votes-Channel");

            foreach (V1Data v1Channel in channels)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Channel.GuildId);
                    ulong channelId = ulong.Parse(v1Channel.Value);

                    SocketGuild guild = Program.Client.GetGuild(guildId);

                    string mode = "All";
                    try { mode = V1Data.GetFirstData(guildId.ToString(), $"Votes-Mode-{channelId}").Value; }
                    catch { try { mode = V1Data.GetFirstData(guildId.ToString(), "Votes-Mode").Value; } catch { } }

                    string upName = EString.FromDecoded("⬆️").EncodedValue;
                    try { upName = V1Data.GetFirstData(guildId.ToString(), $"Votes-UpName-{channelId}").Value; }
                    catch { try { upName = V1Data.GetFirstData(guildId.ToString(), "Votes-UpName").Value; } catch { } }

                    string downName = EString.FromDecoded("⬇️").EncodedValue;
                    try { downName = V1Data.GetFirstData(guildId.ToString(), $"Votes-DownName-{channelId}").Value; }
                    catch { try { downName = V1Data.GetFirstData(guildId.ToString(), "Votes-DownName").Value; } catch { } }

                    IEmote upEmote;
                    if (GetGuildEmote(upName, guild) != null) upEmote = GetGuildEmote(upName, guild);
                    else upEmote = GetDiscordEmote(Base64Decode(upName));

                    IEmote downEmote;
                    if (GetGuildEmote(downName, guild) != null) downEmote = GetGuildEmote(downName, guild);
                    else downEmote = GetDiscordEmote(Base64Decode(downName));

                    VoteChannelsRow row = new VoteChannelsRow(guildId, channelId)
                    {
                        Mode = mode == "Attachments" ? 3 : 0,
                        Emotes = new List<IEmote> {upEmote, downEmote}
                    };

                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }

        private static Emote GetGuildEmote(string input, SocketGuild guild)
        {
            try { return guild.Emotes.First(x => x.Name == input); } catch { }
            try { return guild.Emotes.First(x => x.Name == input.Split(":").ToArray()[1]); } catch { }

            return null;
        }

        private static Emoji GetDiscordEmote(string input)
        {
            try { return new Emoji(input); } catch { }

            return null;
        }

        private static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
