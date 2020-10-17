using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace Utili
{
    internal class Config
    {
        public string Token { get; set; } = "";
        public int LowerShardId { get; set; } = 0;
        public int UpperShardId { get; set; } = 0;

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
}
