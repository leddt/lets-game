using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using LetsGame.Web.Services.Igdb.Models;
using Microsoft.Extensions.Options;

namespace LetsGame.Web.Services.Igdb
{
    public interface IGameSearcher
    {
        Task<Game[]> SearchGamesAsync(string query);
        Task<Game?> GetGameAsync(long id);
    }

    public class IgdbClient(HttpClient client, IOptions<IgdbOptions> options) : IGameSearcher
    {
        private const string DefaultGameFields = "name,screenshots.image_id,artworks.image_id,first_release_date";
        private static IgdbAccessToken? _accessToken;

        private async Task<IgdbAccessToken> GetAccessTokenAsync()
        {
            if (_accessToken?.IsExpired == false) return _accessToken;

            var uri = $"https://id.twitch.tv/oauth2/token?client_id={options.Value.ClientId}&client_secret={options.Value.ClientSecret}&grant_type=client_credentials";
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            _accessToken = await response.Content.ReadFromJsonAsync<IgdbAccessToken>();

            return _accessToken ?? throw new InvalidOperationException("Failed to get access token");
        }

        private bool IsEnabled => !string.IsNullOrWhiteSpace(options.Value.ClientId);

        public async Task<Game[]> SearchGamesAsync(string query)
        {
            if (!IsEnabled) return [GetFakeGame()];
            
            return await IgdbQuery<Game[]>(
                "games",
                fields: DefaultGameFields,
                search: query,
                where: "parent_game = null & version_parent = null",
                limit: 48) ?? [];
        }

        public async Task<Game?> GetGameAsync(long id)
        {
            if (!IsEnabled) return GetFakeGame();
            
            var results = await IgdbQuery<Game[]>(
                "games",
                fields: DefaultGameFields,
                where: $"id = {id}");

            return results?.FirstOrDefault();
        }

        private static Game GetFakeGame() => new()
        {
            Id = 999999,
            Name = "Fake Game"
        };

        private async Task<T?> IgdbQuery<T>(
            string endpoint,
            string? fields = null,
            string? search = null,
            string? where = null,
            int? limit = null)
        {
            var queryBuilder = new StringBuilder();
            AddClause(nameof(fields), fields);
            AddClause(nameof(search), search, quoted: true);
            AddClause(nameof(where), where);
            AddClause(nameof(limit), limit);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Headers.Add("Client-ID", options.Value.ClientId);
            request.Headers.Authorization = AuthenticationHeaderValue.Parse($"Bearer {await GetAccessTokenAsync()}");
            request.Content = new StringContent(queryBuilder.ToString());

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();

            void AddClause(string name, object? value, bool quoted = false)
            {
                if (value == null) return;

                queryBuilder.AppendLine(quoted 
                    ? $"{name} \"{value}\";" 
                    : $"{name} {value};");
            }
        }

        public static void Configure(IServiceProvider services, HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.igdb.com/v4/");
        }
    }
}