using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Pages.Events;

[Authorize(Policy = "CanEditEvent")]
public class Edit : PageModel
{
    public readonly EventAppDbContext Db;


    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } // Event ID
    
    [BindProperty]
    public Event? TheEvent { get; set; }
    
    public bool IsAdmin => User.IsInRole("Admin");
    public bool IsNew => TheEvent?.Id == "N/A";
    
    // [BindProperty]
    public string EventHostUsername { get; set; }

    public Edit(EventAppDbContext db)
    {
        this.Db = db;
    }
    
    public IActionResult OnGet()
    {
        
        // Making a new event
        if (string.IsNullOrEmpty(Id))
        {
            TheEvent = new Event
            {
                Id = "N/A", // ONPOST will check if the ID is "" and create a new GUID
                HostId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };
            return Page();
        }
        
        
        TheEvent = Db.Events.FirstOrDefault(e => e.Id == Id);
        if (TheEvent == null) return RedirectToPage("/Events/Index");
        
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        // Check that the host id is the same as the user id, or if the user is an admin
        if (TheEvent.HostId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
        {
            TempData["BannerMessage"] = "You do not have permission to do that!";
            return RedirectToPage("/Events/View", new { id = TheEvent.Id });
        }
        
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
            Db.SaveChanges();
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
        
        Db.Events.Update(TheEvent);
        Db.SaveChanges();
        TempData["BannerMessage"] = "Event updated successfully!";
        return RedirectToPage("/Events/View", new { id = TheEvent.Id });
    }
}