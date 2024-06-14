using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using evently.Models;

namespace evently.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public Contact ContactInfo { get; set; }
        
        public EventAppDbContext _db { get; set; }

        public ContactModel(EventAppDbContext db)
        {
            _db = db;
        }
        
        public void OnGet() { }

        public IActionResult OnPost()
        {
            if(!ModelState.IsValid) return Page();
            
            _db.Contacts.Add(ContactInfo);
            _db.SaveChanges();
            TempData["BannerMessage"] = "Thank you for your message! We will get back to you soon.";
            
            return RedirectToPage("/Contact"); // Same page, but doing this clears the form
        }
    }
}
