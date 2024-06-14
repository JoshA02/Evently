using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace evently.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        public DateTime CreationDate { get; set; } = new DateTime(1970, 1, 1);
        
        public string FullName => FirstName + " " + LastName;
    }
}
