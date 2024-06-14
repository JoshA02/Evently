using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using evently.Models;

namespace evently.Pages.Events;

[Authorize(Policy = "CanEditEvent")]
public class Edit : PageModel
{
    public readonly EventAppDbContext Db;
    private readonly UserManager<User> _userManager;


    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } // Event ID
    
    [BindProperty]
    public Event? TheEvent { get; set; }
    
    public bool IsAdmin => User.IsInRole("Admin");
    public bool IsNew => TheEvent?.Id == "N/A";
    public bool IsEmailConfirmed = false;
    
    // [BindProperty]
    public string EventHostUsername { get; set; }

    public Edit(EventAppDbContext db, UserManager<User> userManager)
    {
        this.Db = db;
        _userManager = userManager;
    }
    
    public async Task<IActionResult> OnGetAsync()
    {
        string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        User? currentUser = Db.Users.FirstOrDefault(u => u.Id == currentUserId);
        if (currentUser == null) return RedirectToPage("/Account/Manage/Index", new {area = "Identity"});
        
        // Check if the user has confirmed their email
        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(currentUser);
        if (!IsEmailConfirmed)
        {
            TempData["BannerMessage"] = "You must confirm your email before you can create or edit an event.";
            return RedirectToPage("/Account/Manage/Index", new {area = "Identity"});
        }
        
        // Making a new event
        if (string.IsNullOrEmpty(Id))
        {
            TheEvent = new Event
            {
                Id = "N/A", // ONPOST will check if the ID is "" and create a new GUID
                HostId = currentUserId
            };
            return Page();
        }
        
        
        TheEvent = Db.Events.FirstOrDefault(e => e.Id == Id);
        if (TheEvent == null) return RedirectToPage("/Events/Index");
        
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        
        // TheEvent = await Db.Events.FindAsync(TheEvent.Id);
        // if (TheEvent == null) return RedirectToPage("/Events/Index");
        
        // Check that the host id is the same as the user id, or if the user is an admin
        if (TheEvent.HostId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
        {
            TempData["BannerMessage"] = "You do not have permission to do that!";
            return RedirectToPage("/Events/View", new { id = TheEvent.Id });
        }
        
        Console.WriteLine("Stage 1" + Db.Entry(TheEvent).State); // Debugging
        
        // New event
        if (TheEvent.Id == "N/A")
        {
            User? host = Db.Users.FirstOrDefault(u => u.Id == TheEvent.HostId);
            if (host == null)
            {
                TempData["BannerMessage"] = "Host not found. Please double check the host ID.";
                return Page();
            }
            
            TheEvent.Id = Guid.NewGuid().ToString();
            Db.Events.Add(TheEvent);
            await Db.SaveChangesAsync();
            TempData["BannerMessage"] = "Event created successfully!";
            return RedirectToPage("/Events/View", new { id = TheEvent.Id });
        }
        
        if (!ModelState.IsValid)
        {
            // Print why the model is invalid
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            return Page();
        }
        
        if(TheEvent.Capacity < TheEvent.Registered)
        {
            TempData["BannerMessage"] = "Capacity cannot be less than the number of registered attendees!";
            return Page();
        }
        
        // Make sure they either own this event or are an admin
        if (TheEvent.HostId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
        {
            TempData["BannerMessage"] = "You do not have permission to do that!";
            return RedirectToPage("/Events/View", new { id = TheEvent.Id });
        }
        Console.WriteLine("Stage 2" + Db.Entry(TheEvent).State); // Debugging
        
        Db.Events.Update(TheEvent);
        await Db.SaveChangesAsync();
        
        Console.WriteLine("Stage 3" + Db.Entry(TheEvent).State); // Debugging
        
        TempData["BannerMessage"] = "Event updated successfully!";
        return RedirectToPage("/Events/View", new { id = TheEvent.Id });
    }
}