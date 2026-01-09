using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Services.EventSystem;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebPush;

namespace LetsGame.Web.Services
{
    public class SimpleNotificationPayload
    {
        public string Type => "simple";
            
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Image { get; set; }
        public string? Url { get; set; }

        public string Icon { get; set; } = "/favicon.png";
    }

    public interface IPushSender
    {
        Task SendNotification(IEnumerable<string> subscriptions, SimpleNotificationPayload payload);
    }

    public class PushSender(
        WebPushClient webPushClient,
        VapidDetails vapidDetails,
        ILogger<PushSender> logger,
        IEventSystem eventSystem
    )
        : IPushSender
    {
        public async Task SendNotification(IEnumerable<string> subscriptions, SimpleNotificationPayload payload)
        {
            var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            
            try
            {
                await Task.WhenAll(
                    subscriptions.Select(async s =>
                    {
                        await webPushClient.SendNotificationAsync(
                            DeserializeSubscription(s),
                            json,
                            vapidDetails
                        );

                        eventSystem.Enrich(x => x.Increment("PushNotificationsSent"));
                    })
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending notification");
            }
        }

        private PushSubscription DeserializeSubscription(string subscription)
        {
            var details = JsonConvert.DeserializeObject<PushSubscriptionDetails>(subscription)
                          ?? throw new InvalidOperationException("Failed to deserialize subscription");
            var pushSubscription = new PushSubscription(
                details.Endpoint, 
                details.Keys?.P256dh, 
                details.Keys?.Auth);

            return pushSubscription;
        }

        private class PushSubscriptionDetails
        {
            public string? Endpoint { get; set; }
            public PushSubscriptionKeys? Keys { get; set; }

            public class PushSubscriptionKeys
            {
                public string? P256dh { get; set; }
                public string? Auth { get; set; }
            }
        }
    }
}