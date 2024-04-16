using System.ComponentModel.DataAnnotations;

namespace soft20181_starter.Models
{
    public class EventReview
    {
        public int Id { get; set; }
        
        // The user that wrote this review
        public string UserId { get; set; } = "";
        
        
        // The event that this review is for
        public string HostId { get; set; } = "";

        [Required]
        [StringLength(1500, ErrorMessage = "Message must be less than 1500 characters.")]
        public string Message { get; set; } = "";
    }
}
