using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Database
{
    public class Haste
    {
        private string BaseUrl { get; }

        public async Task<string> PasteAsync(string content, string format)
        {
            HttpContent httpContent = new StringContent(content);
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage httpResponse = await httpClient.PostAsync($"{BaseUrl}/documents", httpContent);
            string json = await httpResponse.Content.ReadAsStringAsync();
            string key = JsonConvert.DeserializeObject<PasteResponse>(json).Key;
            return $"{BaseUrl}/{key}.{format}";
        }

        public Haste(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        private class PasteResponse
        {
            [JsonProperty("key")]
            public string Key { get; set; }
        }
    }
}
