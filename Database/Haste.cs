using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Database
{
    public class Haste
    {
        private string BaseUrl { get; set; }

        public async Task<string> PasteAsync(string content, string format)
        {
            HttpContent httpContent = new StringContent(content);
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage httpResponse = await httpClient.PostAsync($"{BaseUrl}/documents", httpContent);
            string key = JsonSerializer.Deserialize<PasteResponse>(await httpResponse.Content.ReadAsStringAsync()).key;
            return $"{BaseUrl}/{key}.{format}";
        }

        public Haste(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PasteResponse
        {
            // ReSharper disable once InconsistentNaming
            public string key { get; set; }
        }
    }
}
