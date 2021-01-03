using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataTransfer.Transfer
{
    class V1Config
    {
        public string V1Connection { get; set; }
        public string V1Token { get; set; }

        public static V1Config Load()
        {
            while (true)
            {
                try
                {
                    string json = File.ReadAllText("V1Config.json");
                    return JsonSerializer.Deserialize<V1Config>(json);
                }
                catch
                {
                    V1Config config = new V1Config {V1Connection = Menu.GetString("v1 connection"), V1Token = Menu.GetString("token")};
                    config.Save();
                }
            }
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions {WriteIndented = true});
            File.WriteAllText("V1Config.json", json);
        }
    }
}
