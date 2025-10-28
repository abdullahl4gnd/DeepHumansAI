using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DeepHumans.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account
{
    public class ForgotPasswordViaCodeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordViaCodeModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public string StatusMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                StatusMessage = "No user found with that email.";
                return Page();
            }

            // Generate a 6-digit random code
            var code = new Random().Next(100000, 999999).ToString();

            // Temporarily store code in user’s security stamp (or in claims)
            user.SecurityStamp = code;
            await _userManager.UpdateAsync(user);

            // Send email
            await _emailSender.SendEmailAsync(
                Input.Email,
                "AI DHumans Password Reset Code",
                $"Your password reset code is: <b>{code}</b><br/><br/>If you didn’t request this, please ignore this email.");

            // Redirect to code entry page
            return RedirectToPage("/Account/ResetPasswordViaCode", new { email = Input.Email });
        }
    }
}
