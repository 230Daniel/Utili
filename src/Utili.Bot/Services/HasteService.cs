using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Utili.Bot.Services;

public class HasteService
{
    private string _baseUrl;

    public async Task<string> PasteAsync(string content, string format)
    {
        HttpContent httpContent = new StringContent(content);
        var httpClient = new HttpClient();

        var httpResponse = await httpClient.PostAsync($"{_baseUrl}/documents", httpContent);
        var json = await httpResponse.Content.ReadAsStringAsync();
        var key = JsonConvert.DeserializeObject<PasteResponse>(json).Key;
        return $"{_baseUrl}/{key}.{format}";
    }

    public HasteService(IConfiguration config)
    {
        _baseUrl = config.GetValue<string>("Services:HasteAddress");
    }

    private class PasteResponse
    {
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}