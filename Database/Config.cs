using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

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
                string filename = "DatabaseCredentials.json";
                string json = File.ReadAllText(filename);

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
