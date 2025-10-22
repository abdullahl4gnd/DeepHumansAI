using System;
using System.ComponentModel.DataAnnotations;

namespace DeepHumans.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; }

        [Required]
        public string CharacterName { get; set; }

        [Required]
        public string MessageContent { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
