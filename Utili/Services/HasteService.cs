using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Utili.Services
{
    public class HasteService
    {
        string _baseUrl;

        public async Task<string> PasteAsync(string content, string format)
        {
            HttpContent httpContent = new StringContent(content);
            HttpClient httpClient = new();

            HttpResponseMessage httpResponse = await httpClient.PostAsync($"{_baseUrl}/documents", httpContent);
            string json = await httpResponse.Content.ReadAsStringAsync();
            string key = JsonConvert.DeserializeObject<PasteResponse>(json).Key;
            return $"{_baseUrl}/{key}.{format}";
        }

        public HasteService(IConfiguration config)
        {
            _baseUrl = config.GetValue<string>("HasteServer");
        }

        class PasteResponse
        {
            [JsonProperty("key")]
            public string Key { get; set; }
        }
    }
}
