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
                StatusMessage = "Session expired. Please request a new password reset.";
                return RedirectToPage("/Account/ForgotPasswordViaCode");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Session expired. Please start again.");
                return RedirectToPage("/Account/ForgotPasswordViaCode");
            }

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don’t reveal whether user exists
                ModelState.AddModelError(string.Empty, "Invalid request.");
                return Page();
            }

            // ✅ ABSOLUTE FINAL CODE FIX FOR CUSTOM FLOW:
            // 1. Directly set the password hash.
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, Input.NewPassword);
            
            // 2. CRITICAL: Force the security stamp to update. This is the key to fixing the "invalid password" error.
            var securityStampResult = await _userManager.UpdateSecurityStampAsync(user);
            
            // 3. Explicitly save the user object with the new password hash and security stamp.
            var result = await _userManager.UpdateAsync(user);

            // We check the result of the final UpdateAsync
            if (result.Succeeded)
            {
                // ✅ Clear session
                HttpContext.Session.Remove("ResetCode");
                HttpContext.Session.Remove("ResetEmail");

                // ✅ Refresh login state (optional)
                await _signInManager.RefreshSignInAsync(user);

                // ✅ Success message shown on page
                StatusMessage = "✅ Your password has been reset successfully. You can now log in with your new password.";
                ModelState.Clear();
                return Page();
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
