using System;
using System.Text.Json.Serialization;

namespace LetsGame.Web.Services.Igdb.Models
{
    public class IgdbAccessToken
    {
        public IgdbAccessToken()
        {
            CreatedAtUtc = DateTime.UtcNow;
        }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        public DateTime CreatedAtUtc { get; }
        public DateTime ExpiresAtUtc => CreatedAtUtc.AddSeconds(ExpiresIn);
        public bool IsExpired => ExpiresAtUtc <= DateTime.UtcNow;

        public override string ToString() => AccessToken;
    }
}