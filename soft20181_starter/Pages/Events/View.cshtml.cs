using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;
    
namespace soft20181_starter.Pages.Events;


[Authorize]
public class View : PageModel
{
    public Event Event { get; set; }
    public string EventHost { get; set; }
    
    [BindProperty]
    public FormInput Input { get; set; }
    
    public readonly EventAppDbContext Db;
    
    public View(EventAppDbContext db)
    {
        this.Db = db;
    }
    public IActionResult OnGet()
    {
        // Url looks like: /Events/View/{id}
        // Get the id from the URL:
        string? id = RouteData.Values["id"]?.ToString() ?? null;
        if (id == null) return RedirectToPage("/Events/Index"); 
        
        // Grab the event from the database
        Event? temp = Db.Events.FirstOrDefault(e => e.Id == id);
        if (temp != null) Event = temp;
        else return RedirectToPage("/Events/Index");
        
        // Grab the host's username from the database
        User? host = Db.Users.FirstOrDefault(u => u.Id == Event.HostId);
        if (host != null) EventHost = host.UserName ?? "Unknown";
        else return RedirectToPage("/Events/Index");
        

        return Page();
    }
    
    public IActionResult OnPost()
    {
        return Page();
        // if (!ModelState.IsValid) return Page();
        //
        // // Url looks like: /Events/View/{id}
        // // Get the id from the URL:
        // string? id = RouteData.Values["id"]?.ToString() ?? null;
        // if (id == null) return RedirectToPage("/Events/Index"); 
        //
        // // Grab the event from the database
        // Event? temp = Db.Events.FirstOrDefault(e => e.Id == id);
        // if (temp != null) Event = temp;
        // else return RedirectToPage("/Events/Index");
        //
        // // Grab the host's username from the database
        // User? host = Db.Users.FirstOrDefault(u => u.Id == Event.HostId);
        // if (host != null) EventHost = host.UserName ?? "Unknown";
        // else return RedirectToPage("/Events/Index");
        //
        // // Send an email to the host
        // string subject = $"RSVP for {Event.Name}";
        // string body = $"Name: {Input.FirstName} {Input.LastName}\n" +
        //               $"Email: {Input.Email}\n" +
        //               $"Phone: {Input.Phone}\n";
        // Email.Send(host.Email, subject, body);
        //
        // return RedirectToPage("/Events/Index");
    }
}

public class FormInput
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int TicketCount { get; set; } = 1;
}