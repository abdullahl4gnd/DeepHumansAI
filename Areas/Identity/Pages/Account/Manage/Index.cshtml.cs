// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DeepHumans.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeepHumans.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            [RegularExpression(@"^\+?\d{6,15}$", ErrorMessage = "Please enter a valid phone number (6–15 digits, optional +).")]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            Username = await _userManager.GetEmailAsync(user);
            Input = new InputModel
            {
                UserName = await _userManager.GetUserNameAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user) ?? string.Empty
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

            // ✅ Update Phone if changed
            var currentPhone = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != currentPhone)
            {
                var result = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!result.Succeeded)
                {
                    StatusMessage = "⚠️ Error: Unable to update phone number.";
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
    }
}
