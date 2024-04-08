using System.ComponentModel.DataAnnotations;

namespace soft20181_starter.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; } // Hashed password
    }
}
