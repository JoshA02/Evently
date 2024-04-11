using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class Index : PageModel
{
    public List<EventBookingWithEvent> EventBookings { get; set; }
    
    public readonly EventAppDbContext Db;
    
    public Index(EventAppDbContext db)
    {
        this.Db = db;
    }
    
    public void OnGet()
    {
        var temp = Db.Bookings.Where(e => e.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value).ToList();
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
    }
}

public class EventBookingWithEvent
{
    public EventBooking Booking { get; set; }
    public Event Event { get; set; }
}