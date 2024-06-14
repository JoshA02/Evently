// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using evently.Models;

namespace evently.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        // private readonly UrlEncoder _urlEncoder;
        private readonly EventAppDbContext _db;
        private readonly IEmailSender _emailSender;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            EventAppDbContext db,
            IEmailSender emailSender
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _emailSender = emailSender;
        }
        
        public bool IsEmailConfirmed = false;
        
        [TempData]
        public string BannerMessage { get; set; }
        
        [BindProperty]
        public InputModel Input { get; set; }

        public List<EventWithHost> HostedEvents = new List<EventWithHost>();
        
        public class InputModel
        {
            [Display(Name = "Username")]
            [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens.")]
            public string Username { get; set; }
            
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }
        
        // This is called for both get and post requests
        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel
            {
                Username = userName,
                PhoneNumber = phoneNumber
            };
            
            
            // Get all events hosted by the user
            List<Event> _HostedEvents = _db.Events
                .Where(e => e.HostId == user.Id)
                .ToList();
            
            foreach (var e in _HostedEvents)
            {
                var host = await _userManager.FindByIdAsync(e.HostId);
                if(host == null) continue;
                HostedEvents.Add(new EventWithHost{Event = e, Host = host});
            }
            
            // Check if the user has confirmed their email:
            var email = await _userManager.GetEmailAsync(user);
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            await LoadAsync(user);
            return Page();
        }
        
        public async Task<IActionResult> OnGetSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code, returnUrl = Url.Content("~/") },
                protocol: Request.Scheme);

            // Grab the email template from ./EmailTemplates/ConfirmEmail.html:
            string emailTemplate = await System.IO.File.ReadAllTextAsync("./EmailTemplates/ConfirmEmail.html");
            emailTemplate = emailTemplate.Replace("{{firstName}}", user.FirstName);
            emailTemplate = emailTemplate.Replace("{{confirmUrl}}", HtmlEncoder.Default.Encode(callbackUrl));

            await _emailSender.SendEmailAsync(user.Email, "Evently | Confirm your email", emailTemplate);
            
            BannerMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userName = await _userManager.GetUserNameAsync(user);
            if (Input.Username != userName)
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(user, Input.Username);
                if (!setUsernameResult.Succeeded)
                {
                    BannerMessage = "Unexpected error when trying to set username.";
                    return RedirectToPage();
                }
            }
            
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    BannerMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            BannerMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLogOutAsync() {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
        public IActionResult OnPostMyBookingsAsync() {
            return RedirectToPage("/Account/Manage/Bookings/Index", new { area = "Identity" });
        }
        public IActionResult OnPostNewEventAsync() {
            return RedirectToPage("/Events/Edit");
        }
    }
}
