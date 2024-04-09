using System.ComponentModel.DataAnnotations;

namespace soft20181_starter.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required] public string FirstName { get; set; } = "";

        [Required] public string LastName { get; set; } = "";

        [Required] [EmailAddress] public string Email { get; set; } = "";

        public string Phone { get; set; } = ""; // Optional field, can be empty or null
        public string EventName { get; set; } = ""; // Optional field, can be empty or null
        public string EventHost { get; set; } = ""; // Optional field, can be empty or null

        [Required] public string Message { get; set; } = "";
    }
}
