using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DeepHumans.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account
{
    public class ResetPasswordViaCodeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordViaCodeModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = TempData["VerifiedEmail"]?.ToString();
            if (email == null)
            {
                StatusMessage = "Session expired. Please start again.";
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                StatusMessage = "User not found.";
                return Page();
            }

            // Remove old password and set new one
            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, Input.NewPassword);

            if (!result.Succeeded)
            {
                StatusMessage = "Failed to reset password.";
                return Page();
            }

            StatusMessage = "Password reset successful! You can now sign in.";
            return RedirectToPage("/Account/Login");
        }
    }
}
