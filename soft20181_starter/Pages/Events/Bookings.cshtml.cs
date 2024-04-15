using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Pages.Events;

[Authorize(Policy = "CanEditEvent")]
public class Bookings : PageModel
{
    
    public readonly EventAppDbContext Db;
    
    public Event Event { get; set; }
    public List<EventBooking> EventBookings { get; set; }
    
    public Bookings(EventAppDbContext db)
    {
        this.Db = db;
    }
    
    public void OnGet(string id)
    {
        Event = Db.Events.FirstOrDefault(e => e.Id.ToString() == id);
        if (Event == null) return;
        
        EventBookings = Db.Bookings.Where(b => b.EventId == Event.Id).ToList();
        
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