using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;

namespace DataTransfer.Transfer
{
    internal static class JoinMessage
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> guilds;
            if(oneGuildId == null) guilds = V1Data.GetDataList(type: "JoinMessage-Enabled");
            else guilds = V1Data.GetDataList(oneGuildId.ToString(), "JoinMessage-Enabled");

            foreach (V1Data v1Channel in guilds)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Channel.GuildId);

                    string channel = "DM";
                    try { channel = V1Data.GetFirstData(guildId.ToString(), "JoinMessage-Channel").Value; } catch { }

                    bool direct = channel == "DM";
                    ulong channelId = ulong.TryParse(channel, out ulong x) ? x : 0;

                    string title = "";
                    try { title = V1Data.GetFirstData(guildId.ToString(), "JoinMessage-Title").Value; } catch { }
                    
                    string content = "";
                    try { content = V1Data.GetFirstData(guildId.ToString(), "JoinMessage-Content").Value; } catch { }

                    string normalText = "";
                    try { normalText = V1Data.GetFirstData(guildId.ToString(), "JoinMessage-NormalText").Value; } catch { }

                    string footer = "";
                    try { footer = V1Data.GetFirstData(guildId.ToString(), "JoinMessage-Footer").Value; } catch { }

                    // In v1 "ImageURL" was the icon url. (next to author in embed)
                    // In v2 "ImageURL" is the embed's large image. "IconURL" replaced the old "ImageURL"

                    string iconUrl = "";
                    try { iconUrl = V1Data.GetFirstData(guildId.ToString(), "JoinMessage-ImageURL").Value; } catch { }

                    string thumbnailUrl = "";
                    try { thumbnailUrl = V1Data.GetFirstData(guildId.ToString(), "JoinMessage-ThumbnailURL").Value; } catch { }

                    string imageUrl = "";
                    try { imageUrl = V1Data.GetFirstData(guildId.ToString(), "JoinMessage-LargeImageURL").Value; } catch { }

                    string colourString = "255 255 255";
                    try { colourString = V1Data.GetFirstData(guildId.ToString(), "JoinMessage-Colour").Value; } catch { }
                    byte r = byte.Parse(colourString.Split(" ").ToArray()[0]);
                    byte g = byte.Parse(colourString.Split(" ").ToArray()[1]); // this code is taken from v1 please forgive <3
                    byte b = byte.Parse(colourString.Split(" ").ToArray()[2]);
                    Color colour = new Color(r, g, b);

                    JoinMessageRow row = JoinMessageRow.FromDatabase(guildId, true, direct, channelId, title, footer,
                        content, normalText, imageUrl, thumbnailUrl, iconUrl, colour.RawValue);

                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
