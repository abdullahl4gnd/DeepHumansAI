using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account
{
    public class VerifyCodeModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "The code must be 6 digits.")]
            public string Code { get; set; }
        }

        public IActionResult OnPost()
        {
            var storedCode = TempData["ResetOTP"]?.ToString();
            var email = TempData["Email"]?.ToString();

            if (storedCode == null || email == null)
            {
                StatusMessage = "Session expired. Please request a new code.";
                return Page();
            }

            if (Input.Code != storedCode)
            {
                StatusMessage = "Invalid verification code.";
                return Page();
            }

            TempData["VerifiedEmail"] = email;
            return RedirectToPage("/Account/ResetPasswordViaCode");
        }
    }
}
