using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Controllers
{
    [Authorize]
    public class MessageController : DefaultController
    {
        private readonly UserManager<WhotUser> _userManager;
        private readonly AppDbContext _context;
        public MessageController(UserManager<WhotUser> userManager, AppDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }
        /// <summary>
        /// POST api/message
        /// </summary>
        /// <param name="messageDTO"></param>
        /// <returns>The newly created <see cref="Message" /> converted to <see cref="MessageDTO" /></returns>
        [HttpPost]
        public async Task<ActionResult<MessageDTO>> SendMessage(MessageDTO messageDTO)
        {
            var sender = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Name).Value);
            var recipient = await _userManager.FindByNameAsync(messageDTO.Recipientname);
            if (recipient == null)
            {
                return BadRequest("Invalid Recepient");
            }
            var message = new Message
            {
                Text = messageDTO.Text,
                Sender = sender,
                Recipient = recipient,
            };
            _context.Messages.Add(message);

            if (await _context.SaveChangesAsync() > 0)
                return MessageToDto(message, recipient.LastActive);

            return BadRequest("Sending Message Failed");
        }

        /// <summary>
        /// GET api/message/{username}
        /// </summary>
        /// <param name="recipientName"></param>
        /// <returns>A <see cref="List{MessageDTO}" /> representing the conversation</returns>
        [HttpGet("{recipientName}")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetConversation(string recipientName)
        {
            var sender = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Name).Value);
            var recipient = await _userManager.FindByNameAsync(recipientName.ToLower().Trim());

            if (recipient == null) return BadRequest("Recipient not found");

            var receivedMessages = await _context.Messages.Include(m => m.Sender).Include(m => m.Recipient)
                                                          .Where(msg => (msg.SenderId == sender.Id && msg.RecipientId == recipient.Id)
                                                                   || (msg.SenderId == recipient.Id && msg.RecipientId == sender.Id))
                                                          .OrderByDescending(m => m.CreatedAt)
                                                          .ToListAsync();

            if (receivedMessages == null) return NoContent();

            List<MessageDTO> returnableMessages = new List<MessageDTO>();

            foreach (var msg in receivedMessages)
                returnableMessages.Add(MessageToDto(msg, recipient.LastActive));
            return Ok(returnableMessages);
        }

        /// <summary>
        /// GET api/message/inbox
        /// </summary>
        /// <returns>A <see cref="List{MessageDTO}" /> representing the Inbox</returns>
        [HttpGet("inbox")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetInbox()
        {
            var userMe = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Name).Value);

            var inboxMessages = await _context.Messages.Include(m => m.Sender).Include(m => m.Recipient)
                                                          .Where(msg => (msg.SenderId == userMe.Id || msg.RecipientId == userMe.Id))
                                                          .OrderByDescending(m => m.CreatedAt)
                                                          .ToListAsync();

            if (inboxMessages == null) return NoContent();

            var usersFound = new List<string>();

            List<MessageDTO> returnableMessages = new List<MessageDTO>();

            foreach (var msg in inboxMessages)
            {
                if (msg.SenderId == userMe.Id)
                {
                    if (usersFound.IndexOf(msg.RecipientId) == -1)
                    {
                        returnableMessages.Add(MessageToDto(msg, msg.Recipient.LastActive));
                        usersFound.Add(msg.RecipientId);
                    }
                }
                if (msg.RecipientId == userMe.Id)
                {
                    if (usersFound.IndexOf(msg.SenderId) == -1)
                    {
                        returnableMessages.Add(MessageToDto(msg, msg.Sender.LastActive));
                        usersFound.Add(msg.SenderId);
                    }
                }
            }

            return Ok(returnableMessages);
        }

        /// <summary>
        /// Utility method.
        /// Converts Message to MessageDTO
        /// </summary>
        /// <param name="message"></param>
        /// <returns><see cref="MessageDTO" /></returns>
        private MessageDTO MessageToDto(Message message, DateTime? lastActive = null)
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
                UserLastActive = lastActive ?? DateTime.UtcNow
            };
        }
    }
}