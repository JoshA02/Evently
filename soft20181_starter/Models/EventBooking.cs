using System.ComponentModel.DataAnnotations;

namespace soft20181_starter.Models;

public class EventBooking
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public string EventId { get; set; }
    
    [Required]
    public int Tickets { get; set; }
    
    [Required]
    public DateTime BookingTime { get; set; } = DateTime.Now;
    
    [Required]
    public float TotalPrice { get; set; }
}