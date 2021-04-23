using System;

namespace Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public WhotUser Sender { get; set; }
        public string SenderId { get; set; }
        public WhotUser Recipient { get; set; }
        public string RecipientId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}