using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using soft20181_starter.Models;

namespace soft20181_starter.Pages.Events;

[Authorize]
public class Edit : PageModel
{
    public readonly EventAppDbContext Db;


    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } // Event ID
    
    [BindProperty]
    public Event theEvent { get; set; }
    
    // [BindProperty]
    public string EventHostUsername { get; set; }

    public Edit(EventAppDbContext db)
    {
        this.Db = db;
    }
    
    public void OnGet()
    {
        theEvent = Db.Events.Find(Id);
        if (theEvent == null) RedirectToPage("/Events/Index");
    }
    
    public IActionResult OnPost()
    {
        // if (!ModelState.IsValid) return Page();
        if (!ModelState.IsValid)
        {
            // Print why the model is invalid
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            return Page();
        }
        
        Db.Events.Update(theEvent);
        Db.SaveChanges();
        return RedirectToPage("/Events/View", new { id = theEvent.Id });
    }
}