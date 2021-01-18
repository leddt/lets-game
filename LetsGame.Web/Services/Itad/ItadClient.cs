using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LetsGame.Web.Services.Itad.Model;
using Microsoft.Extensions.Options;

namespace LetsGame.Web.Services.Itad
{
    public class ItadClient
    {
        private readonly HttpClient _client;
        private readonly IOptions<ItadOptions> _options;

        public ItadClient(HttpClient client, IOptions<ItadOptions> options)
        {
            _client = client;
            _options = options;
        }

        public async Task<ItadSearchResults> SearchAsync(string text, int limit = 20, bool strict = false)
        {
            if (string.IsNullOrWhiteSpace(_options.Value.ApiKey))
                return new ItadSearchResults();

            var uri = $"search/search?key={_options.Value.ApiKey}&q={text}&limit={limit}&strict={(strict ? 1 : 0)}";
            var results = await _client.GetFromJsonAsync<ItadResponse<ItadSearchResults>>(uri);

            return results.Data;
        }

        public static void Configure(IServiceProvider services, HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.isthereanydeal.com/v02/");
        }
    }
}