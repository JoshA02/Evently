using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class Index : PageModel
{
    public List<EventBookingWithEvent> EventBookings { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; }
    
    public User UserToDisplay { get; set; }
    
    public readonly EventAppDbContext Db;
    
    public Index(EventAppDbContext db)
    {
        this.Db = db;
    }
    
    public async Task<IActionResult> OnGet()
    {
        string userIdToUse = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if(User.IsInRole("Admin") && !string.IsNullOrEmpty(UserId)) userIdToUse = UserId;

        UserToDisplay = Db.Users.FirstOrDefault(u => u.Id == userIdToUse);
        if (UserToDisplay == null)
        {
            TempData["BannerMessage"] = "User not found!";
            return RedirectToPage("/Index");
        }
        
        
        var temp = Db.Bookings.Where(e => e.UserId == userIdToUse).ToList();
        EventBookings = new List<EventBookingWithEvent>();
        foreach (var booking in temp)
        {
            Event? tempEvent = Db.Events.FirstOrDefault(e => e.Id == booking.EventId);
            if (tempEvent != null)
            {
                EventBookings.Add(new EventBookingWithEvent
                {
                    Booking = booking,
                    Event = tempEvent
                });
            }
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int bookingId)
    {
        var booking = await Db.Bookings.FindAsync(bookingId);
        if (booking == null) 
        {
            TempData["BannerMessage"] = "Booking not found!";
            return RedirectToPage();
        }

        Event? e = Db.Events.FirstOrDefault(e => e.Id == booking.EventId);
        if (e == null)
        {
            TempData["BannerMessage"] = "Event not found! Removing booking anyway.";
            Db.Bookings.Remove(booking);
            await Db.SaveChangesAsync();
            
            return RedirectToPage();
        }
        
        e.Registered -= booking.Tickets;
        Db.Bookings.Remove(booking);
        await Db.SaveChangesAsync();
        TempData["BannerMessage"] = "Booking deleted!";
        
        return RedirectToPage();
    }
}

public class EventBookingWithEvent
{
    public EventBooking Booking { get; set; }
    public Event Event { get; set; }
}