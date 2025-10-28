using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DeepHumans.Models;
using DeepHumans.Services; // your custom EmailSender
using Microsoft.AspNetCore.Http; // ✅ For HttpContext.Session
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account
{
    public class ForgotPasswordViaCodeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordViaCodeModel(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Security: don’t reveal that the user doesn’t exist
                StatusMessage = "If that email is registered, a verification code has been sent.";
                return RedirectToPage("/Account/VerifyCode");
            }

            // ✅ Reverted: Generate 6-digit code
            var code = new Random().Next(100000, 999999).ToString();

            // ✅ Store in session for later verification
            HttpContext.Session.SetString("ResetCode", code);
            HttpContext.Session.SetString("ResetEmail", Input.Email);
            HttpContext.Session.SetString("CodeTimestamp", DateTime.UtcNow.ToString("O"));

            // ✅ Email content
            var subject = "AI DHumans Password Reset Code";
            var body = $@"
                <h3>Your AI DHumans password reset code:</h3>
                <div style='font-size:24px;font-weight:bold;color:#b71c1c;'>{code}</div>
                  

                <p>This code will expire in 10 minutes.</p>
                <p>If you didn’t request this, please ignore this email.</p>";

            await _emailSender.SendEmailAsync(Input.Email, subject, body);

            StatusMessage = "Verification code sent successfully. Please check your email.";
            return RedirectToPage("/Account/VerifyCode");
        }
    }
}
