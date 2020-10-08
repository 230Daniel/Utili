using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace Utili
{
    class Config
    {
        public string Token { get; set; } = "";
        public int LowerShardId { get; set; } = 0;
        public int UpperShardId { get; set; } = 0;
        public int TotalShards { get; set; } = 1;

        public static Config Load()
        {
            try
            {
                string filename = "Config.json";
                string json = File.ReadAllText(filename);

                Config config = JsonSerializer.Deserialize<Config>(json);

                return config;
            }
            catch (FileNotFoundException e)
            {
                string json = JsonSerializer.Serialize(new Config(), new JsonSerializerOptions{WriteIndented = true});

                File.WriteAllText("Config.json", json);
            }
            catch { }

            return new Config();
        }
    }
}
