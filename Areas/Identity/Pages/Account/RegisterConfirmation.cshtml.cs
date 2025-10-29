using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        public string Email { get; set; } = string.Empty;

        public IActionResult OnGet(string email = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                // If email is missing, just go home
                return RedirectToPage("/Index");
            }

            Email = email;

            // ✅ No email verification — just show success message
            return Page();
        }
    }
}
