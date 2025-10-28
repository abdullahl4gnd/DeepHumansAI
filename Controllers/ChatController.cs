using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeepHumans.Data;
using DeepHumans.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DeepHumans.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                .Select(m => new { m.MessageContent, m.Timestamp, IsUser = true })
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
                Timestamp = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class ChatMessageViewModel
    {
        public string CharacterName { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
    }
}
