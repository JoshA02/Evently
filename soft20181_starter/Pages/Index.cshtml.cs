using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Pages
{
    public class IndexModel : PageModel
    {
        // private readonly ILogger<IndexModel> _logger;

        public readonly EventAppDbContext db;
        
        public List<Event> LatestEvents = new List<Event>();
        public List<Event> CheapestEvents = new List<Event>();

        public IndexModel(EventAppDbContext db)
        {
            this.db = db;
        }

        public void OnGet()
        {
            LatestEvents = db.Events.OrderByDescending(e => e.DateTime).Take(4).ToList();
            CheapestEvents = db.Events.OrderBy(e => e.Price).Take(4).ToList();
        }
    }
}
