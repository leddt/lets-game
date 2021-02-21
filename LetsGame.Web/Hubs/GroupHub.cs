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
        private record Connection(string ConnectionId, string UserId, string GroupId);
        private static readonly List<Connection> Connections = new();
        
        private readonly UserManager<AppUser> _userManager;

        public GroupHub(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(Context.User);

        public async Task Join(string group)
        {
            var currentUserId = GetCurrentUserId();
            
            Connections.Add(new Connection(Context.ConnectionId, currentUserId, group));
            
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.OthersInGroup(group).SendAsync("here", currentUserId);

            var otherUserIds = Connections
                .Where(x => x.GroupId == group)
                .Select(x => x.UserId)
                .Distinct()
                .Where(x => x != currentUserId)
                .ToList();
            
            if (otherUserIds.Any())
            {
                await Clients.Caller.SendAsync("hereToo", otherUserIds);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var currentUserId = GetCurrentUserId();
            var groups = Connections
                .Where(x => x.ConnectionId == Context.ConnectionId)
                .Select(x => x.GroupId)
                .Distinct()
                .ToList();
            
            Connections.RemoveAll(x => x.ConnectionId == Context.ConnectionId);

            foreach (var group in groups)
            {
                if (!Connections.Any(x => x.GroupId == group && x.UserId == currentUserId))
                {
                    Clients.OthersInGroup(group).SendAsync("left", currentUserId);
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}