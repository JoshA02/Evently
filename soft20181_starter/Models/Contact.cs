using System.ComponentModel.DataAnnotations;

namespace soft20181_starter.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"[a-zA-Z]{2,30}", ErrorMessage = "First name must be between 2 and 30 characters and contain only letters.")]
        public string FirstName { get; set; } = "";

        [Required]
        [RegularExpression(@"[a-zA-Z]{2,30}", ErrorMessage = "Last name must be between 2 and 30 characters and contain only letters.")]
        public string LastName { get; set; } = "";

        [Required] [EmailAddress] public string Email { get; set; } = "";

        [Phone] // This is a built-in validation attribute
        public string? Phone { get; set; } = ""; // Optional field, can be empty or null
        
        [StringLength(50, ErrorMessage = "Event name must be less than 50 characters.")]
        public string? EventName { get; set; } = ""; // Optional field, can be empty or null
        
        [StringLength(50, ErrorMessage = "Event host must be less than 50 characters.")]
        public string? EventHost { get; set; } = ""; // Optional field, can be empty or null

        [Required]
        [StringLength(2000, ErrorMessage = "Message must be less than 2000 characters.")]
        public string Message { get; set; } = "";
    }
}
