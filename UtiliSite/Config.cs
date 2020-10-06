using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace Database
{
    internal class Config
    {
        public string DiscordAuthUrl { get; set; }
        public string DiscordClientId { get; set; }
        public string DiscordClientSecret { get; set; }
        public string DiscordToken { get; set; }

        public void Load()
        {
            try
            {
                string filename = "Config.json";
                string json = File.ReadAllText(filename);

                Config config = JsonSerializer.Deserialize<Config>(json);

                DiscordAuthUrl = config.DiscordAuthUrl;
                DiscordClientId = config.DiscordClientId;
                DiscordClientSecret = config.DiscordClientSecret;
                DiscordToken = config.DiscordToken;
            }
            catch (FileNotFoundException e)
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions{WriteIndented = true});

                File.WriteAllText("Config.json", json);
            }
            catch { }
        }
    }
}