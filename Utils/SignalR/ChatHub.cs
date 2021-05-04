using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Data;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Models;

namespace Utils.SignalR
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserManager<WhotUser> _userManager;
        private readonly AppDbContext _context;
        public ChatHub(UserManager<WhotUser> userManager, AppDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.FindFirst(ClaimTypes.Name).Value;
            var httpContext = Context.GetHttpContext();
            var otherguy = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(username, otherguy);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        }
        public async override Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(MessageDTO messageDTO)
        {
            var username = Context.User.FindFirst(ClaimTypes.Name).Value;
            var sender = await _userManager.FindByNameAsync(username);
            var recipient = await _userManager.FindByNameAsync(messageDTO.Recipientname);
            if (recipient == null)
            {
                throw new HubException("Invalid Recepient");
            }
            var message = new Message
            {
                Text = messageDTO.Text,
                Sender = sender,
                Recipient = recipient,
            };
            _context.Messages.Add(message);

            if (await _context.SaveChangesAsync() > 0)
            {
                var groupName = GetGroupName(sender.UserName, recipient.UserName);
                await Clients.Group(groupName).SendAsync("NewMessage", MessageToDto(message, recipient.LastActive));
            }
            else
            {
                throw new HubException("Sending Message Failed");
            }
        }
        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
        /// <summary>
        /// Utility method.
        /// Converts Message to MessageDTO
        /// </summary>
        /// <param name="message"></param>
        /// <returns><see cref="MessageDTO" /></returns>
        private MessageDTO MessageToDto(Message message, DateTime? lastActive)
        {
            return new MessageDTO
            {
                Id = message.Id,
                Text = message.Text,
                Sendername = message.Sender.UserName,
                SenderId = message.Sender.Id,
                Recipientname = message.Recipient.UserName,
                RecipientId = message.Recipient.Id,
                CreatedAt = message.CreatedAt,
                UserLastActive = lastActive
            };
        }
    }
}