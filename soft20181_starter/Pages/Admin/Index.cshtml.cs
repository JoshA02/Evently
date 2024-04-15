using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PostmarkDotNet.Exceptions;
using soft20181_starter.Models;

namespace soft20181_starter.Pages.Admin;

[Authorize(Roles = "Admin")]
public class Index : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? UserSearchQuery { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? EventSearchQuery { get; set; }
    
    public List<User> Users { get; set; } = new List<User>();
    public List<Event> Events { get; set; } = new List<Event>();
    // public string StatusMessage { get; set; } = "";
    
    public readonly EventAppDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IUserStore<User> _userStore;
    private readonly SignInManager<User> _signInManager;

    // public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender)
    
    public Index(EventAppDbContext db, UserManager<User> userManager, IEmailSender emailSender, IUserStore<User> userStore, SignInManager<User> signInManager)
    {
        this._db = db;
        _userManager = userManager;
        _emailSender = emailSender;
        _userStore = userStore;
        _signInManager = signInManager;
    }
    
    public void OnGetAsync()
    {
        
        // User Search
        if (string.IsNullOrEmpty(UserSearchQuery) || !ModelState.IsValid)
        {
            Users = _db.Users.OrderByDescending(u => u.CreationDate).Take(50).ToList();
            TempData["StatusMessage"] = "Showing the first 50 users, sorted by creation date.";
        }
        else
        {
            UserSearchQuery = UserSearchQuery.ToLower();
            // Console.WriteLine($"Searching for users with email or name containing '{UserSearchQuery}'");
            Users = _db.Users.OrderByDescending(u => u.CreationDate).Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(UserSearchQuery)) ||
                (u.UserName != null && (u.UserName).ToLower().Contains(UserSearchQuery))
            ).ToList();

            TempData["StatusMessage"] =
                $"Showing users with email or name containing '{UserSearchQuery}', sorted by creation date.";
        }
        
        // Event Search
        if (string.IsNullOrEmpty(EventSearchQuery) || !ModelState.IsValid)
        {
            Events = _db.Events.OrderByDescending(e => e.DateTime).Take(50).ToList();
            TempData["EventStatusMessage"] = "Showing the first 50 events, sorted by date occurring.";
        }
        else
        {
            EventSearchQuery = EventSearchQuery.ToLower();
            Events = _db.Events.OrderByDescending(e => e.DateTime).Where(e =>
                (e.Name.ToLower().Contains(EventSearchQuery)) ||
                (_db.Users.FirstOrDefault(u => u.Id == e.HostId) != null && 
                 _db.Users.First(u => u.Id == e.HostId).UserName.ToLower().Contains(EventSearchQuery))
            ).ToList();

            TempData["EventStatusMessage"] =
                $"Showing events with name or host name containing '{EventSearchQuery}', sorted by date occurring.";
        }
    }
    
    public async Task<IActionResult> OnPostDeleteUserAsync(string id, string previousSearchQuery, string previousEventSearchQuery)
    {
        string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        
        if(currentUserId == "")
        {
            TempData["BannerMessage"] = "Deletion not successful: Could not perform validation check.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
        }
        
        if (currentUserId == id)
        {
            TempData["BannerMessage"] = "Deletion not successful: You cannot delete yourself.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
        }
        
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            TempData["BannerMessage"] = "Deletion not successful: User not found.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        
        TempData["BannerMessage"] = "User deleted successfully";
        return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery }); // This will redirect to the same page with the same search query in the URL
    }
    
    public async Task<IActionResult> OnPostResetPasswordAsync(string id, string previousSearchQuery, string previousEventSearchQuery)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            TempData["BannerMessage"] = "Password reset NOT sent; user not found.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
        }
        if (user.Email == null)
        {
            TempData["BannerMessage"] = "Password reset NOT sent; user has no set email.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
        }
        
        // Consider checking for email confirmation here. If not confirmed, don't send the email.
        
        String currentUserFirstName = _db.Users.Find(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)?.FirstName ?? "Admin";
        
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code },
            protocol: Request.Scheme);
        
        // Grab the email template from ./EmailTemplates/AdminRequestedPasswordReset.html:
        string emailTemplate = System.IO.File.ReadAllText("./EmailTemplates/AdminRequestedPasswordReset.html");
        emailTemplate = emailTemplate.Replace("{{firstName}}", user.FirstName);
        emailTemplate = emailTemplate.Replace("{{resetLink}}", callbackUrl);
        emailTemplate = emailTemplate.Replace("{{signedBy}}", currentUserFirstName);

        await _emailSender.SendEmailAsync(
            user.Email,
            "Reset Password",
            emailTemplate);
        
        TempData["BannerMessage"] = "Sent password reset email to " + user.Email;
        return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery }); // This will redirect to the same page with the same search query in the URL
    }

    // Handles all user updates that require hitting the save button (ones that might be more sensitive)
    public async Task<IActionResult> OnPostSaveUserAsync(string id, string role, string username, string email, string previousSearchQuery, string previousEventSearchQuery)
    {
        var user = await _db.Users.FindAsync(id);
        var thisUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        bool changesMade = false;
        
        if (user == null)
        {
            TempData["BannerMessage"] = "User not found. Could not save changes.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery }); // This will redirect to the same page with the same search query in the URL
        }
        
        
        // ROLE CHANGES BEGIN //
        if(role == "user") role = "visitor";
        if(role != "visitor" && role != "admin")
        {
            TempData["BannerMessage"] = "Invalid role. Could not save changes.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
        }
        if (!_userManager.IsInRoleAsync(user, role).Result)
        {
            if (user.Id == thisUserId)
            {
                TempData["BannerMessage"] = "You cannot change your own role.";
                return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
            }
            
            // Remove them from the other role
            await _userManager.RemoveFromRoleAsync(user, role == "admin" ? "visitor" : "admin");
            
            // Add them to the new role
            await _userManager.AddToRoleAsync(user, role);
            
            // Might need to sign the user out
            
            // Used for the UI message
            changesMade = true;
        }
        // ROLE CHANGES END //
        
        // USERNAME CHANGES BEGIN //
        if(user.NormalizedUserName != username.ToUpper())
        {
            // Check if a user already exists with the new username
            var userWithNewUsername = await _userManager.FindByNameAsync(username);
            if (userWithNewUsername != null)
            {
                TempData["BannerMessage"] = "Username already taken. Could not save changes.";
                return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
            }
            
            await _userStore.SetUserNameAsync(user, username, CancellationToken.None);
            await _userStore.SetNormalizedUserNameAsync(user, username.ToUpper(), CancellationToken.None);
            await _db.SaveChangesAsync(); // This is necessary to update the database with the new username
            
            // Might need to sign the user out
            
            changesMade = true;
        }
        // USERNAME CHANGES END //
        
        // EMAIL CHANGES BEGIN //
        if(user.NormalizedEmail != email.ToUpper())
        {
            // Check if a user already exists with the new email
            var userWithNewEmail = await _userManager.FindByEmailAsync(email);
            if (userWithNewEmail != null)
            {
                TempData["BannerMessage"] = "Email already taken. Could not save changes.";
                return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
            }

            // Inform them of the change via their old email
            if (user.Email != null)
            {
                string emailTemplate = System.IO.File.ReadAllText("./EmailTemplates/EmailChange.html");
                emailTemplate = emailTemplate.Replace("{{firstName}}", user.FirstName);
                emailTemplate = emailTemplate.Replace("{{newEmail}}", email);
                try
                {
                    await _emailSender.SendEmailAsync(user.Email, "Email Change Confirmation", emailTemplate);
                }
                catch (PostmarkValidationException e) { }
            }

            user.Email = email.ToLower();
            user.NormalizedEmail = email.ToUpper();
            await _db.SaveChangesAsync();
            
            // Might need to sign the user out
            
            changesMade = true;
        }
        // EMAIL CHANGES END //
        
        TempData["BannerMessage"] = changesMade ? "User saved successfully" : "No changes to save";
        return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
    }
    
    public async Task<IActionResult> OnPostDeleteEventAsync(string id, string previousSearchQuery, string previousEventSearchQuery)
    {
        var eventToDelete = await _db.Events.FindAsync(id);
        if (eventToDelete == null)
        {
            TempData["BannerMessage"] = "Event not found. Could not delete.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
        }
            
        _db.Events.Remove(eventToDelete);
        await _db.SaveChangesAsync();
            
        TempData["BannerMessage"] = "Event deleted successfully";
        return RedirectToPage(new { UserSearchQuery = previousSearchQuery, EventSearchQuery = previousEventSearchQuery });
    }
}