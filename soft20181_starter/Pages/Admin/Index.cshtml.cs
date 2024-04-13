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
    public string? userSearchQuery { get; set; }
    
    public List<User> Users { get; set; } = new List<User>();
    // public string StatusMessage { get; set; } = "";
    
    public readonly EventAppDbContext db;
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    // public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender)
    
    public Index(EventAppDbContext db, UserManager<User> userManager, IEmailSender emailSender)
    {
        this.db = db;
        _userManager = userManager;
        _emailSender = emailSender;
    }
    
    public void OnGetAsync()
    {
        if (string.IsNullOrEmpty(userSearchQuery) || !ModelState.IsValid)
        {
            Users = db.Users.OrderByDescending(u => u.CreationDate).Take(50).ToList();
            TempData["StatusMessage"] = "Showing the first 50 users, sorted by creation date.";
            return;
        }
        
        userSearchQuery = userSearchQuery.ToLower();
        Console.WriteLine($"Searching for users with email or name containing '{userSearchQuery}'");
        
        Users = db.Users.Where(u =>
            u.Email.ToLower().Contains(userSearchQuery) ||
            (u.UserName).ToLower().Contains(userSearchQuery)
        ).ToList();
        
        TempData["StatusMessage"] = $"Showing users with email or name containing '{userSearchQuery}'.";
    }
    
    public async Task<IActionResult> OnPostDeleteUserAsync(string id, string previousSearchQuery)
    {
        string currentUserID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        
        if(currentUserID == "")
        {
            TempData["BannerMessage"] = "Deletion not successful: Could not perform validation check.";
            return RedirectToPage(new { userSearchQuery = previousSearchQuery });
        }
        
        if (currentUserID == id)
        {
            TempData["BannerMessage"] = "Deletion not successful: You cannot delete yourself.";
            return RedirectToPage(new { userSearchQuery = previousSearchQuery });
        }
        
        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            TempData["BannerMessage"] = "Deletion not successful: User not found.";
            return RedirectToPage(new { userSearchQuery = previousSearchQuery });
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        
        TempData["BannerMessage"] = "User deleted successfully";
        return RedirectToPage(new { userSearchQuery = previousSearchQuery }); // This will redirect to the same page with the same search query in the URL
    }
    
    public async Task<IActionResult> OnPostResetPasswordAsync(string id, string previousSearchQuery)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            TempData["BannerMessage"] = "Password reset NOT sent; user not found.";
            return RedirectToPage(new { userSearchQuery = previousSearchQuery });
        }
        
        // Consider checking for email confirmation here. If not confirmed, don't send the email.
        
        String currentUserFirstName = db.Users.Find(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)?.FirstName ?? "Admin";
        
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code },
            protocol: Request.Scheme);

        await _emailSender.SendEmailAsync(
            user.Email,
            "Reset Password",
            $"Hi {user.FirstName}! Here's a link to reset your password: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>reset password</a>." +
            $"\nKind regards, \n" +
            $"{currentUserFirstName} from Evently.");
        
        
        
        
        TempData["BannerMessage"] = "Sent password reset email to " + user.Email;
        return RedirectToPage(new { userSearchQuery = previousSearchQuery }); // This will redirect to the same page with the same search query in the URL
    }
}