using System.Threading.Tasks;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LetsGame.Web.Controllers
{
    [ApiController]
    public class PushApiController(IPushSender sender) : ControllerBase
    {
        [HttpPost("api/push/test")]
        public async Task<IActionResult> Test(TestNotificationRequest request)
        {
            await sender.SendNotification([request.Subscription], new SimpleNotificationPayload
            {
                Title = "Test notification",
                Body = "It works!"
            });

            return Ok();
        }

        public class TestNotificationRequest
        {
            public required string Subscription { get; set; }
        }
    }
}