using System;
using System.Text.Json.Serialization;
using NodaTime;

namespace LetsGame.Web.Services.Igdb.Models
{
    public class IgdbAccessToken
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }

        public Instant CreatedAt { get; } = SystemClock.Instance.GetCurrentInstant();
        public Instant ExpiresAt => CreatedAt + Duration.FromSeconds(ExpiresIn);
        public bool IsExpired => ExpiresAt <= SystemClock.Instance.GetCurrentInstant();

        public override string ToString() => AccessToken;
    }
}