using System.Text.Json.Serialization;

namespace LetsGame.Web.Services.Igdb.Models
{
    public class Image
    {
        [JsonPropertyName("image_id")]
        public string ImageId { get; set; }

        public string ScreenshotMedUrl => GetScreenshotMedUrl(ImageId);
        public string Url(string format) => GetImageUrl(ImageId, format);

        public static string GetScreenshotMedUrl(string imageId) => GetImageUrl(imageId, "screenshot_med");
        public static string GetCoverBigUrl(string imageId) => GetImageUrl(imageId, "cover_big");
        public static string GetImageUrl(string imageId, string format)
        {
            return string.IsNullOrWhiteSpace(imageId)
                ? null
                : $"https://images.igdb.com/igdb/image/upload/t_{format}/{imageId}.jpg";
        }
    }
}