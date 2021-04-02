using System.Threading.Tasks;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LetsGame.Web.Controllers
{
    [ApiController]
    public class PushApiController : ControllerBase
    {
        private readonly IPushSender _sender;

        public PushApiController(IPushSender sender)
        {
            _sender = sender;
        }

        [HttpPost("api/push/test")]
        public async Task<IActionResult> Test(TestNotificationRequest request)
        {
            await _sender.SendNotification(new[] {request.Subscription}, new SimpleNotificationPayload
            {
                Title = "Test notification",
                Body = "It works!"
            });

            return Ok();
        }

        public class TestNotificationRequest
        {
            public string Subscription { get; set; }
        }
    }
}