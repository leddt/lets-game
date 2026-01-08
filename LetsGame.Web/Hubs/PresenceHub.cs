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
    public interface IPresenceClient
    {
        Task UpdatePresences(string group, IEnumerable<string> userIds);
    }

    public class PresenceCache
    {
        private readonly List<(string connectionId, string userId, string group)> _presences = new();

        public IEnumerable<string> Add(string connectionId, string userId, string group)
        {
            _presences.Add((connectionId, userId, group));
            return GetUsersInGroup(group);
        }

        public IEnumerable<string> Remove(string connectionId, string userId, string group)
        {
            _presences.Remove((connectionId, userId, group));
            return GetUsersInGroup(group);
        }

        private IEnumerable<string> GetUsersInGroup(string group)
        {
            return _presences.Where(x => x.group == group).Select(x => x.userId).Distinct().ToList();
        }

        public IEnumerable<string> GetGroupsByConnectionId(string connectionId)
        {
            return _presences.Where(x => x.connectionId == connectionId).Select(x => x.group).ToList();
        }
    }
    
    [Authorize]
    public class PresenceHub : Hub<IPresenceClient>
    {
        private readonly PresenceCache _presences;
        private readonly UserManager<AppUser> _userManager;

        public PresenceHub(PresenceCache presences, UserManager<AppUser> userManager)
        {
            _presences = presences;
            _userManager = userManager;
        }

        public async Task Join(string group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            if (Context.User is null) return;
            var userId = _userManager.GetUserId(Context.User);
            if (userId is null) return;
            
            var presentUsers = _presences.Add(Context.ConnectionId, userId, group);
            await Clients.Group(group).UpdatePresences(group, presentUsers);
        }

        public async Task Leave(string group)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

            if (Context.User is null) return;
            var userId = _userManager.GetUserId(Context.User);
            if (userId is null) return;
            
            var presentUsers = _presences.Remove(Context.ConnectionId, userId, group);
            await Clients.Group(group).UpdatePresences(group, presentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User is null) return;
            var userId = _userManager.GetUserId(Context.User);
            if (userId is null) return;
            
            var groups = _presences.GetGroupsByConnectionId(Context.ConnectionId);

            foreach (var group in groups)
            {
                var presentUsers = _presences.Remove(Context.ConnectionId, userId, group);
                await Clients.Group(group).UpdatePresences(group, presentUsers);
            }
        }
    }
}