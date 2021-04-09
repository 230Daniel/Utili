using System;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace Utili
{
    internal static class Helper
    {
        public static LogSeverity ConvertToLocalLogSeverity(Discord.LogSeverity severity)
        {
            return severity switch
            {
                Discord.LogSeverity.Critical => LogSeverity.Crit,
                Discord.LogSeverity.Error => LogSeverity.Errr,
                Discord.LogSeverity.Warning => LogSeverity.Warn,
                Discord.LogSeverity.Info => LogSeverity.Info,
                Discord.LogSeverity.Verbose => LogSeverity.Dbug,
                Discord.LogSeverity.Debug => LogSeverity.Dbug,
                _ => throw new Exception("What the heckin heck is this log severity bro?"),
            };
        }

        public static DiscordSocketClient GetShardForGuild(IGuild guild)
        {
            return Program._oldClient.GetShardFor(guild);
        }

        public static IEmote GetEmote(string emoteString, SocketGuild guild)
        {
            if (Emote.TryParse(emoteString, out Emote emote))
                return emote;
            if (guild.Emotes.Any(x => x.Name == emoteString || $":{x.Name}:" == emoteString))
                return guild.Emotes.First(x => x.Name == emoteString || $":{x.Name}:" == emoteString);

            return new Emoji(emoteString);
        }

        public static bool RequiresUpdate(SocketVoiceState before, SocketVoiceState after)
        {
            if (before.VoiceChannel is null && after.VoiceChannel is null)
            {
                return false;
            }

            if (before.VoiceChannel is null || after.VoiceChannel is null)
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