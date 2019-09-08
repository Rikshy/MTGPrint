using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace MTGPrint.Models
{
    public class ScryfallClient
    {
        private const string BASE_URL = "https://api.scryfall.com/";

        private readonly HttpClient client = new HttpClient();

        public ScryfallClient()
        {
            client.BaseAddress = new Uri(BASE_URL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public BulkBase GetBulkInfo() { return Get<BulkBase>("bulk-data"); }

        private T Get<T>(string endpoint, params string[] parameters) where T : class
        {
            var url = endpoint;
            if (parameters.Any())
                url += "?" + string.Join("&", parameters);

            var response = client.GetAsync(url).Result;

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
        }
    }
}