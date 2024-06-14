using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace evently.Models
{
    public class Event
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; } = "";
        
        // Host of the event (User)
        [Required]
        public string HostId { get; set; }
        
        // Navigation property for the host
        // [Required] [ForeignKey("HostId")] public virtual User Host { get; set; }
        
        [Required]
        public string Description { get; set; } = "";
        
        [Required]
        public string ImageUrl { get; set; } = "";
        
        
        public string? VideoUrl { get; set; } = "";
        
        [Required]
        public string Location { get; set; } = "";
        
        [Required]
        public DateTime DateTime { get; set; } = DateTime.Now;
        
        [Required]
        public int Capacity { get; set; } = 0; // Total seats
        
        [Required]
        public int Registered { get; set; } = 0; // Seats filled
        
        [Required]
        public float Price { get; set; } = 0.0f; // Price per seat (in GBP)
    }
}
