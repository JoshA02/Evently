using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Pages.Events;

public class Index : PageModel
{
    public List<EventWithHost> Events = new List<EventWithHost>();
    
    public readonly EventAppDbContext db;
    
    public string query { get; set; }
    
    private User dummyUser;

    public Index(EventAppDbContext db)
    {
        this.db = db;
        dummyUser = db.Users.First();
    }

    public void OnGet(string q)
    {
        query = q ?? "";
        
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? dummyUser.Id;
        
        // Get all events from the database, sorted by date, from the current date onwards, and filtered by the query string (if any):
        List<Event> _Events = db.Events.OrderBy(e => e.DateTime).Where(e => e.Name.ToLower().Contains(query.ToLower()) && e.DateTime > DateTime.Now).ToList();
        foreach (var e in _Events)
        {
            User host = db.Users.Find(e.HostId);
            if (host == null) continue;
            Events.Add(new EventWithHost { Event = e, Host = host });
        }
        
        // Temp: Add 10 random events to the list:
        for (int i = 0; i < 10; i++)
        {
            // If the db already has 10 events, don't add more:
            if(Events.Count >= 10) break;
            
            DateTime ThreeToEightMonthsFromNow = DateTime.Now.AddMonths(new Random().Next(3, 8));
            ThreeToEightMonthsFromNow = ThreeToEightMonthsFromNow.AddHours(new Random().Next(1, 24));
            ThreeToEightMonthsFromNow = ThreeToEightMonthsFromNow.AddDays(new Random().Next(1, 30));
            
            Event newEvent = new Event
            {
                Id = Guid.NewGuid().ToString(),
                Name = "New Event ",
                DateTime = ThreeToEightMonthsFromNow,
                HostId = userId
            };
            db.Events.Add(newEvent);
        }
        db.SaveChanges();
        
    }
    
    // public void OnGet()
    // {
    //     // Read the 'q' string from the query parameters in the url:
    //     
    //     
    //     Console.WriteLine(q);
    //     // Event newEvent = new Event
    //     // {
    //     //     Id = Guid.NewGuid().ToString(),
    //     //     Name = "New Event " + DateTime.Now,
    //     //     DateTime = DateTime.Now,
    //     //     HostId = dummyUser.Id
    //     // };
    //     // db.Events.Add(newEvent);
    //     // db.SaveChanges();
    //     if(q == null) q = "";
    //
    //     Events = db.Events.Where(e => e.Name.ToLower().Contains(q.ToLower())).ToList();
    //
    //     foreach (var e in Events)
    //     {
    //         Console.Out.WriteLine(e.HostId);
    //     }
    // }
}