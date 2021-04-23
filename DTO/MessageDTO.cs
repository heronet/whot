using System;

namespace DTO
{
    public class MessageDTO
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Sendername { get; set; }
        public string SenderId { get; set; }
        public string Recipientname { get; set; }
        public string RecipientId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}