using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;

namespace DataTransfer.Transfer
{
    internal static class Notices
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> channels;
            if(oneGuildId == null) channels = V1Data.GetDataList(type: "Notices-Channel");
            else channels = V1Data.GetDataList(oneGuildId.ToString(), "Notices-Channel");

            foreach (V1Data v1Channel in channels)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Channel.GuildId);
                    ulong channelId = ulong.Parse(v1Channel.Value);

                    TimeSpan delay = TimeSpan.FromSeconds(15);
                    try{ delay = TimeSpan.Parse(V1Data.GetFirstData(guildId.ToString(), $"Notices-Delay-{channelId}").Value); } catch { }

                    ulong messageId = 0;
                    try{ messageId = ulong.Parse(V1Data.GetFirstData(guildId.ToString(), $"Notices-Message-{channelId}").Value); } catch { }

                    string title = "";
                    try { title = V1Data.GetFirstData(guildId.ToString(), $"Notices-Title-{channelId}").Value; } catch { }
                    
                    string content = "";
                    try { content = V1Data.GetFirstData(guildId.ToString(), $"Notices-Content-{channelId}").Value; } catch { }

                    string normalText = "";
                    try { normalText = V1Data.GetFirstData(guildId.ToString(), $"Notices-NormalText-{channelId}").Value; } catch { }

                    string footer = "";
                    try { footer = V1Data.GetFirstData(guildId.ToString(), $"Notices-Footer-{channelId}").Value; } catch { }

                    // In v1 "ImageURL" was the icon url. (next to author in embed)
                    // In v2 "ImageURL" is the embed's large image. "IconURL" replaced the old "ImageURL"

                    string iconUrl = "";
                    try { iconUrl = V1Data.GetFirstData(guildId.ToString(), $"Notices-ImageURL-{channelId}").Value; } catch { }

                    string thumbnailUrl = "";
                    try { thumbnailUrl = V1Data.GetFirstData(guildId.ToString(), $"Notices-ThumbnailURL-{channelId}").Value; } catch { }

                    string imageUrl = "";
                    try { imageUrl = V1Data.GetFirstData(guildId.ToString(), $"Notices-LargeImageURL-{channelId}").Value; } catch { }

                    string colourString = "255 255 255";
                    try { colourString = V1Data.GetFirstData(guildId.ToString(), $"Notices-Colour-{channelId}").Value; } catch { }
                    byte r = byte.Parse(colourString.Split(" ").ToArray()[0]);
                    byte g = byte.Parse(colourString.Split(" ").ToArray()[1]); // this code is taken from v1 please forgive <3
                    byte b = byte.Parse(colourString.Split(" ").ToArray()[2]);
                    Color colour = new Color(r, g, b);

                    NoticesRow row = NoticesRow.FromDatabase(guildId, channelId, messageId, true, delay.ToString(),
                        title, footer, content, normalText, imageUrl, thumbnailUrl, iconUrl, colour.RawValue);
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
