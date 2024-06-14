using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace evently.Models
{
    public class EventAppDbContext : IdentityDbContext<User>
    {
        public EventAppDbContext(DbContextOptions<EventAppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Contact> Contacts { get; set; } // Table for contact form submissions
        public DbSet<Event> Events { get; set; }
        
        public DbSet<EventBooking> Bookings { get; set; }
        public DbSet<EventReview> Reviews { get; set; }
    }

}
