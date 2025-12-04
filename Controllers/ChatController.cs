using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeepHumans.Data;
using DeepHumans.Models;
using System.Linq;
using System.Threading.Tasks;
using DeepHumans.Services;

namespace DeepHumans.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAssistantService _assistant;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAssistantService assistant)
        {
            _context = context;
            _userManager = userManager;
            _assistant = assistant;
        }

        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string characterName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var chatHistory = await _context.ChatMessages
                .Where(m => m.UserId == user.Id && m.CharacterName == characterName)
                .OrderBy(m => m.Timestamp)
                .Select(m => new { m.Id, m.MessageContent, m.Timestamp, IsUser = !m.IsBot })
                .ToListAsync();

            return Json(chatHistory);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var chatMessage = new ChatMessage
            {
                UserId = user.Id,
                CharacterName = model.CharacterName,
                MessageContent = model.MessageContent,
                IsBot = false,
                Timestamp = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            string reply;
            try
            {
                // Call AI assistant for a reply
                reply = await _assistant.GetReplyAsync(model.CharacterName, model.MessageContent);
                
                // Save bot reply to database
                var botMessage = new ChatMessage
                {
                    UserId = user.Id,
                    CharacterName = model.CharacterName,
                    MessageContent = reply,
                    IsBot = true,
                    Timestamp = DateTime.UtcNow
                };
                _context.ChatMessages.Add(botMessage);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                reply = $"AI request failed: {ex.Message}";
            }

            // Return bot reply
            return Ok(new { reply });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.UserId == user.Id);

            if (message == null)
            {
                return NotFound();
            }

            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearChat(string characterName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var messages = await _context.ChatMessages
                .Where(m => m.UserId == user.Id && m.CharacterName == characterName)
                .ToListAsync();

            _context.ChatMessages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }

    public class ChatMessageViewModel
    {
        public string CharacterName { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
    }
}
