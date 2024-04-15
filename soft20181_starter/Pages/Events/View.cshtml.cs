using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Pages.Events;

[Authorize]
public class View : PageModel
{
    public Event Event { get; set; }
    public string EventHost { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public FormInput Input { get; set; }
    
    const int SUGGESTED_EVENT_COUNT = 4;
    
    public List<Event> SuggestedEvents { get; set; }
    
    public readonly EventAppDbContext Db;
    
    public View(EventAppDbContext db)
    {
        this.Db = db;
    }
    public IActionResult OnGet()
    {
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
        
        // This user
        User? user = Db.Users.Find(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        if (user == null) return RedirectToPage("/Events/Index");
        
        // Set default values for the form
        Input = new FormInput
        {
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            Phone = User.FindFirst(ClaimTypes.MobilePhone)?.Value ?? "",
            TicketCount = 1,
            EventId = Event.Id,
        };
        
        // Get suggested events - START
        
        List<Event> LatestEvents = Db.Events.OrderBy(e => e.DateTime).Where(e => 
            e.DateTime > DateTime.Now &&
            e.HostId != user.Id &&
            e.Id != Event.Id
        ).Take(20).ToList();

        List<EventBooking> UserBookings = Db.Bookings.Where(b => b.UserId == user.Id).ToList();
        List<string> BookedHosts = new List<string>();
        List<string> BookedEventIds = new List<string>();
        foreach (EventBooking booking in UserBookings)
        {
            Event? e = Db.Events.FirstOrDefault(e => e.Id == booking.EventId);
            if (e != null && !BookedHosts.Contains(e.HostId)) BookedHosts.Add(e.HostId);
            if (e != null && !BookedEventIds.Contains(e.Id)) BookedEventIds.Add(e.Id);
        }
        
        List<Event> RelevantEvents = Db.Events.OrderBy(e => e.DateTime).Where(e => 
            e.DateTime > DateTime.Now &&
            e.Id != Event.Id &&
            e.HostId != user.Id &&
            BookedHosts.Contains(e.HostId) &&
            !BookedEventIds.Contains(e.Id)
        ).Take(4).ToList();

        SuggestedEvents = RelevantEvents;

        Console.WriteLine("Need to fill " + (SUGGESTED_EVENT_COUNT - SuggestedEvents.Count) + " more suggested events");
        
        // If we don't have enough events, fill the rest with the latest events
        foreach (Event e in LatestEvents)
        {
            if (SuggestedEvents.Count >= SUGGESTED_EVENT_COUNT) break;
            if (!SuggestedEvents.Contains(e)) SuggestedEvents.Add(e);
        }

        // Get suggested events - END
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostBookAsync() {
        
        // Url looks like: /Events/View/{id}
        // Get the id from the URL:
        string? id = RouteData.Values["id"]?.ToString() ?? null;
        if (id == null) return RedirectToPage("/Events/Index"); 
        
        // Grab the event from the database
        Event? temp = Db.Events.FirstOrDefault(e => e.Id == id);
        if (temp != null) Event = temp;
        else return NotFound("Event not found");

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            Console.Out.WriteLine(Input.TicketCount);
            
            // Create a new booking
            EventBooking booking = new EventBooking
            {
                UserId = userId,
                EventId = Event.Id,
                Tickets = Input.TicketCount,
                TotalPrice = Input.TicketCount * Event.Price
            };

            // Add the booking to the database
            Db.Bookings.Add(booking);
            
            Event.Registered += Input.TicketCount;
            Db.Events.Update(Event);
            
            await Db.SaveChangesAsync();
            return RedirectToPage("/Account/Manage/Bookings/Index", new {area = "Identity"});
        } catch (Exception e)
        {
            return RedirectToPage("/Events/Index");
        }
    }
}

public class FormInput
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int TicketCount { get; set; }
    public string EventId { get; set; }
}