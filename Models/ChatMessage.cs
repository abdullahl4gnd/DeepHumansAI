using System;
using System.ComponentModel.DataAnnotations;

namespace DeepHumans.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public string CharacterName { get; set; } = string.Empty;

        [Required]
        public string MessageContent { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
    }
}
