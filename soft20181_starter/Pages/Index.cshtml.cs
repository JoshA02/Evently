using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Pages
{
    public class IndexModel : PageModel
    {
        // private readonly ILogger<IndexModel> _logger;

        public readonly EventAppDbContext db;
        
        public List<EventWithHost> LatestEvents = new List<EventWithHost>();
        public List<EventWithHost> CheapestEvents = new List<EventWithHost>();

        public IndexModel(EventAppDbContext db)
        {
            this.db = db;
        }

        public void OnGet()
        {
            List<Event> _LatestEvents = db.Events.OrderBy(e => e.DateTime).Where(e => e.DateTime > DateTime.Now).Take(3).ToList();
            List<Event> _CheapestEvents = db.Events.OrderBy(e => e.Price).Take(3).ToList();
            
            foreach (Event e in _LatestEvents)
            {
                User host = db.Users.FirstOrDefault(u => u.Id == e.HostId);
                LatestEvents.Add(new EventWithHost { Event = e, Host = host });
            }
            
            foreach (Event e in _CheapestEvents)
            {
                User host = db.Users.FirstOrDefault(u => u.Id == e.HostId);
                CheapestEvents.Add(new EventWithHost { Event = e, Host = host });
            }
        }
    }
}
