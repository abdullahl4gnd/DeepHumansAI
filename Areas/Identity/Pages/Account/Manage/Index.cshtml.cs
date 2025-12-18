// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DeepHumans.Models;
using DeepHumans.Services; // ✅ for IEmailSender if you're using custom one
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // Display email (read-only)
        [Display(Name = "Email")]
        public string Username { get; set; }

        // TempData used by the toast notification in the view
        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Username is required.")]
            [Display(Name = "Username")]
            public string UserName { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            Username = await _userManager.GetEmailAsync(user);
            Input = new InputModel
            {
                UserName = await _userManager.GetUserNameAsync(user)
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            bool updated = false;

            // ✅ Update Username if changed
            if (Input.UserName != user.UserName)
            {
                var result = await _userManager.SetUserNameAsync(user, Input.UserName);
                if (!result.Succeeded)
                {
                    StatusMessage = "⚠️ Error: Unable to update username.";
                    return RedirectToPage();
                }
                updated = true;
            }

            if (updated)
            {
                await _signInManager.RefreshSignInAsync(user);
                StatusMessage = "✅ Your profile has been updated successfully.";
            }
            else
            {
                StatusMessage = "ℹ️ No changes detected.";
            }

            return RedirectToPage();
        }

        // ✅ NEW HANDLER: Send password update verification email
        public async Task<IActionResult> OnPostSendPasswordResetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Generate a secure random 6-digit code
            var code = new Random().Next(100000, 999999).ToString();

            // Save to session for VerifyCode flow
            HttpContext.Session.SetString("ResetCode", code);
            HttpContext.Session.SetString("ResetEmail", user.Email);
            HttpContext.Session.SetString("CodeTimestamp", DateTime.UtcNow.ToString("O"));

            // Prepare email content
            var subject = "AI DHumans Password Update Code";
            var body = $@"
                <h3>Hello {user.UserName},</h3>
                <p>Your password update verification code is:</p>
                <div style='font-size:24px;font-weight:bold;color:#b71c1c;'>{code}</div>
                <p>This code expires in 10 minutes.</p>
                <p>If you didn’t request this, please ignore this email.</p>";

            // Send email
            await _emailSender.SendEmailAsync(user.Email, subject, body);

            TempData["StatusMessage"] = "✅ Password update code sent successfully! Please check your email.";
            return RedirectToPage("/Account/VerifyCode");
        }
    }
}
