using System;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace Utili
{
    internal class Helper
    {
        public static LogSeverity ConvertToLocalLogSeverity(Discord.LogSeverity severity)
        {
            return severity switch
            {
                Discord.LogSeverity.Critical => LogSeverity.Error,
                Discord.LogSeverity.Error => LogSeverity.Error,
                Discord.LogSeverity.Warning => LogSeverity.Warn,
                Discord.LogSeverity.Info => LogSeverity.Info,
                Discord.LogSeverity.Verbose => LogSeverity.Dbug,
                Discord.LogSeverity.Debug => LogSeverity.Dbug,
                _ => throw new Exception("What the heckin heck is this log severity bro?"),
            };
        }

        public static DiscordSocketClient GetShardForGuild(IGuild guild)
        {
            return Program._client.GetShardFor(guild);
        }

        public static IEmote GetEmote(string input, SocketGuild guild)
        {
            try
            {
                return guild.Emotes.First(x => x.Name == input);
            } 
            catch { }

            try
            {
                return guild.Emotes.First(x => x.Name == input.Split(":").ElementAt(1));
            } 
            catch { }

            try
            {
                return new Emoji(input);
            } 
            catch { }

            return null;
        }

        public static bool RequiresUpdate(SocketVoiceState before, SocketVoiceState after)
        {
            if (before.VoiceChannel == null && after.VoiceChannel == null)
            {
                return false;
            }

            if (before.VoiceChannel == null && after.VoiceChannel != null)
            {
                return true;
            }

            if (after.VoiceChannel == null && before.VoiceChannel != null)
            {
                return true;
            }

            return after.VoiceChannel.Id != before.VoiceChannel.Id;
        }

        public static string EncodeString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        public static string DecodeString(string input)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(input);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return input;
            }
        }

        public static string ToUniversalDateTime(DateTime dt)
        {
            return $"{dt.Year}-{dt.Month}-{dt.Day} {dt.Hour}:{dt.Minute}:{dt.Second}";
        }
    }
}