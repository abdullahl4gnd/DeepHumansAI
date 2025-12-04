using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeepHumans.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        // The ID of the user who sent the message
        [Required]
        public string UserId { get; set; } = string.Empty;

        // The name of the AI character or person being chatted with
        [Required]
        public string CharacterName { get; set; } = string.Empty;

        // The content of the message
        [Required]
        public string MessageContent { get; set; } = string.Empty;

        // Is this message from the bot/AI?
        public bool IsBot { get; set; } = false;

        // Timestamp for when the message was sent
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
