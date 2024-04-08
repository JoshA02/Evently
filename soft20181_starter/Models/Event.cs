using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace soft20181_starter.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";
        
        // Host of the event (User)
        [Required]
        [ForeignKey("Id")]
        public virtual User Host { get; set; }
        
        [Required]
        public string Description { get; set; } = "";
        
        [Required]
        public string ImageUrl { get; set; } = "";

        [Required]
        public string VideoUrl { get; set; } = "";
        
        [Required]
        public string Location { get; set; } = "";
        
        [Required]
        public string DateTime { get; set; } = "";
        
        [Required]
        public int Capacity { get; set; } = 0; // Total seats
        
        [Required]
        public int Registered { get; set; } = 0; // Seats filled
        
        [Required]
        public float Price { get; set; } = 0.0f; // Price per seat (in GBP)
    }
}
