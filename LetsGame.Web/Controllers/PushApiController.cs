using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WebPush;

namespace LetsGame.Web.Controllers
{
    [ApiController]
    public class PushApiController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PushApiController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("api/push/test")]
        public async Task<IActionResult> Test(TestNotificationRequest request)
        {
            var details = JsonConvert.DeserializeObject<PushSubscriptionDetails>(request.Subscription);
            var pushSubscription = new PushSubscription(details.Endpoint, details.Keys.P256dh, details.Keys.Auth);

            var client = new WebPushClient();
            var vapidDetails = new VapidDetails(_config["vapid:subject"], _config["vapid:publicKey"], _config["vapid:privateKey"]);

            var payload = JsonConvert.SerializeObject(new
            {
                title = "Test notification",
                body = "It works!"
            });
            
            await client.SendNotificationAsync(pushSubscription, payload, vapidDetails);

            return Ok();
        }

        private class PushSubscriptionDetails
        {
            public string Endpoint { get; set; }
            public PushSubscriptionKeys Keys { get; set; }

            public class PushSubscriptionKeys
            {
                public string P256dh { get; set; }
                public string Auth { get; set; }
            }
        }

        public class TestNotificationRequest
        {
            public string Subscription { get; set; }
        }
    }
}