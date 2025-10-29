using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DeepHumans.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account
{
    public class ResetPasswordViaCodeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ResetPasswordViaCodeModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            [StringLength(100, ErrorMessage = "Password must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                StatusMessage = "Your session has expired. Please request a new password reset.";
                return RedirectToPage("/Account/ForgotPasswordViaCode");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = HttpContext.Session.GetString("ResetEmail");

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Session expired. Please request a new reset link.");
                return RedirectToPage("/Account/ForgotPasswordViaCode");
            }

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't expose existence of user
                ModelState.AddModelError(string.Empty, "Invalid request.");
                return Page();
            }

            // Retrieve the stored verification code and timestamp
            var storedCode = HttpContext.Session.GetString("ResetCode");
            var codeTimestamp = HttpContext.Session.GetString("CodeTimestamp");

            // ✅ Ensure a code exists and hasn’t expired (10 min window)
            if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(codeTimestamp))
            {
                StatusMessage = "Verification expired. Please request a new code.";
                return RedirectToPage("/Account/ForgotPasswordViaCode");
            }

            if (DateTime.TryParse(codeTimestamp, out var timestamp))
            {
                if (DateTime.UtcNow - timestamp > TimeSpan.FromMinutes(10))
                {
                    StatusMessage = "Verification code expired. Please request a new one.";
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Account/ForgotPasswordViaCode");
                }
            }

            // ✅ Update the password securely
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, Input.NewPassword);

            // ✅ Refresh security stamp to invalidate any old sessions
            await _userManager.UpdateSecurityStampAsync(user);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // ✅ Clear session after successful reset
                HttpContext.Session.Remove("ResetCode");
                HttpContext.Session.Remove("ResetEmail");
                HttpContext.Session.Remove("CodeTimestamp");

                // Optional: sign the user in immediately
                // await _signInManager.SignInAsync(user, isPersistent: false);

                StatusMessage = "✅ Your password has been successfully reset. You can now log in with your new password.";

                // ✅ Redirect to login page with success message
                TempData["SuccessMessage"] = "Password reset successfully. Please log in with your new password.";
                return RedirectToPage("/Account/Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
