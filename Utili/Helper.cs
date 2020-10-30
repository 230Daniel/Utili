using Discord;
using Discord.WebSocket;
using System;
using System.Linq;

namespace Utili
{
    internal class Helper
    {
        public static LogSeverity ConvertToLocalLogSeverity(Discord.LogSeverity severity)
        {
            switch (severity)
            {
                case Discord.LogSeverity.Critical:
                    return LogSeverity.Error;
                case Discord.LogSeverity.Error:
                    return LogSeverity.Error;
                case Discord.LogSeverity.Warning:
                    return LogSeverity.Warn;
                case Discord.LogSeverity.Info:
                    return LogSeverity.Info;
                case Discord.LogSeverity.Verbose:
                    return LogSeverity.Dbug;
                case Discord.LogSeverity.Debug:
                    return LogSeverity.Dbug;
                default:
                    throw new Exception("What the heckin heck is this log severity bro?");
            }
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
    }
}