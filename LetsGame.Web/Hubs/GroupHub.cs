using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace LetsGame.Web.Hubs
{
    [Authorize]
    public class GroupHub : Hub
    {
        private static List<Connection> _connections = new List<Connection>();
        private readonly UserManager<AppUser> _userManager;

        public GroupHub(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(Context.User);

        public async Task Join(string group)
        {
            var currentUserId = GetCurrentUserId();
            
            _connections.Add(new Connection(Context.ConnectionId, currentUserId, group));
            
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.OthersInGroup(group).SendAsync("here", currentUserId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var currentUserId = GetCurrentUserId();
            var groups = _connections.Where(x => x.ConnectionId == Context.ConnectionId).Select(x => x.GroupId).ToList();
            
            _connections.RemoveAll(x => x.ConnectionId == Context.ConnectionId);

            foreach (var group in groups)
            {
                if (!_connections.Any(x => x.GroupId == group && x.UserId == currentUserId))
                {
                    Clients.OthersInGroup(group).SendAsync("left", currentUserId);
                }
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task HereReply(string replyToUserId)
        {
            await Clients.User(replyToUserId).SendAsync("hereToo", GetCurrentUserId());
        }

        private record Connection(string ConnectionId, string UserId, string GroupId);
    }
}