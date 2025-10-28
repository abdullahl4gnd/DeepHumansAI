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
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ResetPasswordViaCodeModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Code { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string email = null)
        {
            Input = new InputModel { Email = email };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "No user found with that email.");
                return Page();
            }

            // Verify code
            if (user.SecurityStamp != Input.Code)
            {
                ModelState.AddModelError("", "Invalid code. Please check your email.");
                return Page();
            }

            // Reset password
            var removePassword = await _userManager.RemovePasswordAsync(user);
            if (!removePassword.Succeeded)
            {
                ModelState.AddModelError("", "Failed to reset password.");
                return Page();
            }

            var addPassword = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            if (addPassword.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToPage("/Account/Login");
            }

            foreach (var error in addPassword.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
