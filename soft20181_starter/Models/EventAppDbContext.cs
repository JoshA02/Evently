﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace soft20181_starter.Models
{
    public class EventAppDbContext : IdentityDbContext<User>
    {
        public EventAppDbContext(DbContextOptions<EventAppDbContext> options) : base(options)
        {
            
        }

        // public DbSet<Contact> Contacts { get; set; } // Table for contact form submissions
    }

}
