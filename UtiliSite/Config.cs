using System.IO;
using System.Text.Json;

namespace Database
{
    internal class Config
    {
        public string DiscordClientId { get; set; }
        public string DiscordClientSecret { get; set; }
        public string DiscordToken { get; set; }
        public string DefaultPrefix { get; set; }

        public void Load()
        {
            try
            {
                string json = File.ReadAllText("Config.json");
                Config config = JsonSerializer.Deserialize<Config>(json);

                DiscordClientId = config.DiscordClientId;
                DiscordClientSecret = config.DiscordClientSecret;
                DiscordToken = config.DiscordToken;
                DefaultPrefix = config.DefaultPrefix;
            }
            catch (FileNotFoundException)
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions{WriteIndented = true});

                File.WriteAllText("Config.json", json);
            }
            catch { }
        }
    }
}