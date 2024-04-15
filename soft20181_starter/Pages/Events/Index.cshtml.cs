using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Pages.Events;

public class Index : PageModel
{
    public List<Event> Events = new List<Event>();
    
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
        
        // Get all events from the database, sorted by date, from the current date onwards, and filtered by the query string (if any):
        Events = db.Events.OrderBy(e => e.DateTime).Where(e => e.Name.ToLower().Contains(query.ToLower()) && e.DateTime > DateTime.Now).ToList();
        
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