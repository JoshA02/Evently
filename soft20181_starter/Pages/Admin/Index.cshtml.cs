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
using soft20181_starter.Models;

namespace soft20181_starter.Pages.Admin;

[Authorize(Roles = "Admin")]
public class Index : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? UserSearchQuery { get; set; }
    
    public List<User> Users { get; set; } = new List<User>();
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
        if (string.IsNullOrEmpty(UserSearchQuery) || !ModelState.IsValid)
        {
            Users = _db.Users.OrderByDescending(u => u.CreationDate).Take(50).ToList();
            TempData["StatusMessage"] = "Showing the first 50 users, sorted by creation date.";
            return;
        }
        
        UserSearchQuery = UserSearchQuery.ToLower();
        Console.WriteLine($"Searching for users with email or name containing '{UserSearchQuery}'");
        
        Users = _db.Users.Where(u =>
            (u.Email != null && u.Email.ToLower().Contains(UserSearchQuery)) ||
            (u.UserName != null && (u.UserName).ToLower().Contains(UserSearchQuery))
        ).ToList();
        
        TempData["StatusMessage"] = $"Showing users with email or name containing '{UserSearchQuery}'.";
    }
    
    public async Task<IActionResult> OnPostDeleteUserAsync(string id, string previousSearchQuery)
    {
        string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        
        if(currentUserId == "")
        {
            TempData["BannerMessage"] = "Deletion not successful: Could not perform validation check.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery });
        }
        
        if (currentUserId == id)
        {
            TempData["BannerMessage"] = "Deletion not successful: You cannot delete yourself.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery });
        }
        
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            TempData["BannerMessage"] = "Deletion not successful: User not found.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery });
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        
        TempData["BannerMessage"] = "User deleted successfully";
        return RedirectToPage(new { UserSearchQuery = previousSearchQuery }); // This will redirect to the same page with the same search query in the URL
    }
    
    public async Task<IActionResult> OnPostResetPasswordAsync(string id, string previousSearchQuery)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            TempData["BannerMessage"] = "Password reset NOT sent; user not found.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery });
        }
        if (user.Email == null)
        {
            TempData["BannerMessage"] = "Password reset NOT sent; user has no set email.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery });
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
        return RedirectToPage(new { UserSearchQuery = previousSearchQuery }); // This will redirect to the same page with the same search query in the URL
    }

    // Handles all user updates that require hitting the save button (ones that might be more sensitive)
    public async Task<IActionResult> OnPostSaveUserAsync(string id, string role, string username, string previousSearchQuery)
    {
        var user = await _db.Users.FindAsync(id);
        bool changesMade = false;
        
        if (user == null)
        {
            TempData["BannerMessage"] = "User not found. Could not save changes.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery });
        }
        if(role == "user") role = "visitor";
        if(role != "visitor" && role != "admin")
        {
            TempData["BannerMessage"] = "Invalid role. Could not save changes.";
            return RedirectToPage(new { UserSearchQuery = previousSearchQuery });
        }

        if (!_userManager.IsInRoleAsync(user, role).Result)
        {
            // Remove them from the other role
            await _userManager.RemoveFromRoleAsync(user, role == "admin" ? "visitor" : "admin");
            
            // Add them to the new role
            await _userManager.AddToRoleAsync(user, role);
            
            // Used for the UI message
            changesMade = true;
        }
        if(user.NormalizedUserName != username.ToUpper())
        {
            // Check if a user already exists with the new username
            var userWithNewUsername = await _userManager.FindByNameAsync(username);
            if (userWithNewUsername != null)
            {
                TempData["BannerMessage"] = "Username already taken. Could not save changes.";
                return RedirectToPage(new { UserSearchQuery = previousSearchQuery });
            }
            
            await _userStore.SetUserNameAsync(user, username, CancellationToken.None);
            await _userStore.SetNormalizedUserNameAsync(user, username.ToUpper(), CancellationToken.None);
            await _db.SaveChangesAsync(); // This is necessary to update the database with the new username
            await _signInManager.RefreshSignInAsync(user); // This is necessary to update the cookie with the new username (I think)
            changesMade = true;
        }
        
        
        
        
        TempData["BannerMessage"] = changesMade ? "User saved successfully" : "No changes to save";
        return RedirectToPage(new { UserSearchQuery = previousSearchQuery });
    }
}