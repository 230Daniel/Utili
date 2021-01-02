using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class MessageFilter
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> images;
            if(oneGuildId == null) images = V1Data.GetDataList(type: "Filter-Images");
            else images = V1Data.GetDataList(oneGuildId.ToString(), "Filter-Images");

            List<V1Data> videos;
            if(oneGuildId == null) videos = V1Data.GetDataList(type: "Filter-Videos");
            else videos = V1Data.GetDataList(oneGuildId.ToString(), "Filter-Videos");

            List<V1Data> media;
            if(oneGuildId == null) media = V1Data.GetDataList(type: "Filter-Media");
            else media = V1Data.GetDataList(oneGuildId.ToString(), "Filter-Media");

            List<V1Data> music;
            if(oneGuildId == null) music = V1Data.GetDataList(type: "Filter-Music");
            else music = V1Data.GetDataList(oneGuildId.ToString(), "Filter-Music");

            List<V1Data> attachments;
            if(oneGuildId == null) attachments = V1Data.GetDataList(type: "Filter-Attachments");
            else attachments = V1Data.GetDataList(oneGuildId.ToString(), "Filter-Attachments");

            List<V1Data> channels = images.Concat(videos).Concat(media).Concat(music).Concat(attachments).ToList();

            foreach (V1Data v1Channel in channels)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Channel.GuildId);
                    ulong channelId = ulong.Parse(v1Channel.Value);

                    int mode = v1Channel.Type switch
                    {
                        "Filter-Images" => 1,
                        "Filter-Videos" => 2,
                        "Filter-Media" => 3,
                        "Filter-Music" => 4,
                        "Filter-Attachments" => 5,
                        _ => 0
                    };

                    MessageFilterRow row = MessageFilterRow.FromDatabase(guildId, channelId, mode, "");
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
