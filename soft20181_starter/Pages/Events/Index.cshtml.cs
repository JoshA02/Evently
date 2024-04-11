using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Pages.Events;

public class Index : PageModel
{
    public List<Event> Events = new List<Event>();
    
    public readonly EventAppDbContext db;

    [BindProperty(SupportsGet = true)] public string q { get; set; } = "";
    
    private User dummyUser;

    public Index(EventAppDbContext db)
    {
        this.db = db;
        dummyUser = db.Users.First();
    }
    
    public void OnGet()
    {
        // Event newEvent = new Event
        // {
        //     Id = Guid.NewGuid().ToString(),
        //     Name = "New Event " + DateTime.Now,
        //     DateTime = DateTime.Now,
        //     HostId = dummyUser.Id
        // };
        // db.Events.Add(newEvent);
        // db.SaveChanges();

        Events = db.Events.Where(e => e.Name.ToLower().Contains(q.ToLower())).ToList();

        foreach (var e in Events)
        {
            Console.Out.WriteLine(e.HostId);
        }
    }
}