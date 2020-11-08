using System.IO;
using System.Text.Json;

namespace Database
{
    class Config
    {
        public string Server { get; set; } = "";
        public string Database { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public void Load()
        {
            try
            {
                string json = File.ReadAllText("DatabaseCredentials.json");

                Config config = JsonSerializer.Deserialize<Config>(json);

                Server = config.Server;
                Database = config.Database;
                Username = config.Username;
                Password = config.Password;
            }
            catch (FileNotFoundException e)
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions{WriteIndented = true});

                File.WriteAllText("DatabaseCredentials.json", json);
            }
            catch { }
        }
    }
}
