using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace soft20181_starter.Pages.Events;

[Authorize]
public class Edit : PageModel
{
    public void OnGet()
    {
        
    }
}