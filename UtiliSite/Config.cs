using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace Database
{
    internal class Config
    {
        public string DiscordClientId { get; set; }
        public string DiscordClientSecret { get; set; }
        public string DiscordToken { get; set; }

        public void Load()
        {
            try
            {
                string json = File.ReadAllText("Config.json");

                Config config = JsonSerializer.Deserialize<Config>(json);

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