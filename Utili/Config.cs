using System.IO;
using System.Text.Json;

namespace Utili
{
    internal class Config
    {
        public string Token { get; set; } = "";
        public bool Production { get; set; } = true;
        public bool CacheDatabase { get; set; } = true;
        public int LowerShardId { get; set; } = 0;
        public int UpperShardId { get; set; } = 0;
        public string Domain { get; set; } = "";
        public string HasteServer { get; set; } = "";
        public bool LogCommands { get; set; } = false;
        public ulong SystemGuildId { get; set; } = 0;
        public ulong SystemChannelId { get; set; } = 0;
        public CommunityConfig Community { get; set; } = new CommunityConfig();
        public string DefaultPrefix { get; set; } = ".";

        public static Config Load()
        {
            try
            {
                string json = File.ReadAllText("Config.json");

                Config config = JsonSerializer.Deserialize<Config>(json);

                return config;
            }
            catch (FileNotFoundException)
            {
                string json = JsonSerializer.Serialize(new Config(), new JsonSerializerOptions{WriteIndented = true});

                File.WriteAllText("Config.json", json);
            }
            catch { }

            return new Config();
        }
    }

    internal class CommunityConfig
    {
        public ulong GuildId { get; set; } = 0;
        public ulong UserRoleId { get; set; } = 0;
        public ulong PremiumRoleId { get; set; } = 0;
    }
}
