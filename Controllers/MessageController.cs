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

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> SendMessage(MessageDTO messageDTO)
        {
            var sender = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Name).Value);
            var recipient = await _userManager.FindByNameAsync(messageDTO.Recipientname);

            var message = new Message
            {
                Text = messageDTO.Text,
                Sender = sender,
                Recipient = recipient,
            };
            _context.Messages.Add(message);

            if (await _context.SaveChangesAsync() > 0)
                return MessageToDto(message);

            return BadRequest("Sending Message Failed");
        }

        [HttpGet("{recipientName}")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetConversation(string recipientName)
        {
            var sender = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Name).Value);
            var recipient = await _userManager.FindByNameAsync(recipientName);

            if (recipient == null) return BadRequest("Recipient not found");

            var receivedMessages = await _context.Messages.Include(m => m.Sender).Include(m => m.Recipient)
                                                          .Where(msg => (msg.SenderId == sender.Id && msg.RecipientId == recipient.Id)
                                                                   || (msg.SenderId == recipient.Id && msg.RecipientId == sender.Id)).ToListAsync();

            if (receivedMessages == null) return NoContent();

            List<MessageDTO> returnableMessages = new List<MessageDTO>();

            foreach (var msg in receivedMessages)
                returnableMessages.Add(MessageToDto(msg));
            return Ok(returnableMessages);
        }


        private MessageDTO MessageToDto(Message message)
        {
            return new MessageDTO
            {
                Id = message.Id,
                Text = message.Text,
                Sendername = message.Sender.UserName,
                SenderId = message.Sender.Id,
                Recipientname = message.Recipient.UserName,
                RecipientId = message.Recipient.Id,
                CreatedAt = message.CreatedAt
            };
        }
    }
}