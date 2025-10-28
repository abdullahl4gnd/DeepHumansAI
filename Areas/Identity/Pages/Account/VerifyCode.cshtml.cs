using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account
{
    public class VerifyCodeModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "The code must be 6 digits.")]
            public string Code { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            // Optionally check if session exists before showing page
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                StatusMessage = "Session expired. Please request a new code.";
            }
        }

        public IActionResult OnPost()
        {
            var storedCode = HttpContext.Session.GetString("ResetCode");
            var storedEmail = HttpContext.Session.GetString("ResetEmail");
            var storedTimeString = HttpContext.Session.GetString("CodeTimestamp");

            if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(storedEmail))
            {
                StatusMessage = "Session expired. Please request a new code.";
                return Page();
            }

            // ✅ Check expiration (10 minutes)
            if (DateTime.TryParse(storedTimeString, out var sentTime))
            {
                if (DateTime.UtcNow - sentTime > TimeSpan.FromMinutes(10))
                {
                    StatusMessage = "The verification code has expired. Please request a new one.";
                    HttpContext.Session.Clear();
                    return Page();
                }
            }

            // ✅ Compare codes
            if (!string.Equals(Input.Code?.Trim(), storedCode, StringComparison.Ordinal))
            {
                StatusMessage = "Invalid verification code. Please try again.";
                return Page();
            }

            // ✅ Code is valid → allow password reset
            HttpContext.Session.SetString("VerifiedEmail", storedEmail);
            return RedirectToPage("/Account/ResetPasswordViaCode", new { email = storedEmail });
        }
    }
}
