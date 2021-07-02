using System.IO;
using System.Text.Json;

namespace Database
{
    public class Config
    {
        public string Server { get; set; } = "";
        public int Port { get; set; }
        public string Database { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string DefaultPrefix { get; set; } = "";

        public void Load()
        {
            try
            {
                var json = File.ReadAllText("DatabaseCredentials.json");

                var config = JsonSerializer.Deserialize<Config>(json);

                Server = config.Server;
                Port = config.Port;
                Database = config.Database;
                Username = config.Username;
                Password = config.Password;
            }
            catch (FileNotFoundException)
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions{WriteIndented = true});

                File.WriteAllText("DatabaseCredentials.json", json);
            }
            catch { }
        }
    }
}
