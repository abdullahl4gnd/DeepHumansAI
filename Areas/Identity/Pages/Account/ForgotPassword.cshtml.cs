using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DeepHumans.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                StatusMessage = "If this email exists, a code has been sent.";
                return Page();
            }

            // Generate a random 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Store it temporarily (you can store it in DB or Redis for production)
            TempData["ResetOTP"] = otp;
            TempData["Email"] = Input.Email;

            // Send it via email
            await _emailSender.SendEmailAsync(Input.Email, "Password Reset Code",
                $"Your verification code is: <b>{otp}</b>. It expires in 10 minutes.");

            StatusMessage = "A verification code has been sent to your email.";
            return RedirectToPage("/Account/VerifyCode");
        }
    }
}
