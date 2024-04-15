using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class View : PageModel
{
    public EventBookingWithEvent booking { get; set; }
    
    public readonly EventAppDbContext Db;
    
    public View(EventAppDbContext db)
    {
        this.Db = db;
    }
    
    public IActionResult OnGet()
    {
        var id = RouteData.Values["id"]?.ToString() ?? null;
        if (id == null) return RedirectToPage("/Account/Manage/Bookings/Index");
        
        booking = new EventBookingWithEvent();
        booking.Booking = Db.Bookings.FirstOrDefault(b => b.Id.ToString() == id);

        if (booking.Booking == null)
        {
            TempData["BannerMessage"] = "Booking not found!";
            return RedirectToPage("/Account/Manage/Bookings/Index");
        }

        if (!User.IsInRole("Admin") && booking.Booking.UserId != User.FindFirst(ClaimTypes.NameIdentifier).Value)
        {
            // TempData["BannerMessage"] = "You do not have permission to view this booking!";
            TempData["BannerMessage"] = "Booking not found!"; // Don't let them know this booking exists
            return RedirectToPage("/Account/Manage/Bookings/Index");
        }
        
        booking.Event = Db.Events.FirstOrDefault(e => e.Id == booking.Booking.EventId);
        if (booking.Event == null) return RedirectToPage("/Account/Manage/Bookings/Index");
        
        return Page();
    }
}