using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Models;

namespace Utils.SignalR
{
    public class ActiveHub : Hub
    {
        private readonly ActiveTracker _tracker;
        private readonly UserManager<WhotUser> _userManager;
        public ActiveHub(ActiveTracker tracker, UserManager<WhotUser> userManager)
        {
            _userManager = userManager;
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var username = Context.User.FindFirst(ClaimTypes.Name).Value;
            await _tracker.UserConnected(username, Context.ConnectionId);
            var activeUsers = await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetActiveUsers", activeUsers);
        }
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var username = Context.User.FindFirst(ClaimTypes.Name).Value;
            var user = await _userManager.FindByNameAsync(username);
            user.LastActive = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _tracker.UserDisconnected(username, Context.ConnectionId);
            var activeUsers = await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetActiveUsers", activeUsers);

            await base.OnDisconnectedAsync(exception);
        }
    }
}