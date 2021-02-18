using System.IO;
using System.Text.Json;

namespace UtiliBackend
{
    internal class Config
    {
        public string Frontend { get; set; }

        public string DiscordClientId { get; set; }
        public string DiscordClientSecret { get; set; }
        public string DiscordToken { get; set; }
        public string DefaultPrefix { get; set; }

        public string StripePrivateKey { get; set; }
        public string StripePublicKey { get; set; }
        public string StripeWebhookSecret { get; set; }
        
        public static Config Load()
        {
            try
            {
                string json = File.ReadAllText("Config.json");
                return JsonSerializer.Deserialize<Config>(json);
            }
            catch (FileNotFoundException)
            {
                string json = JsonSerializer.Serialize(new Config(), new JsonSerializerOptions{WriteIndented = true});
                File.WriteAllText("Config.json", json);
            }

            return new Config();
        }
    }
}