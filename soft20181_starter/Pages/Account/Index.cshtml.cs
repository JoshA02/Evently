using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace soft20181_starter.Pages.Account;

[Authorize]
public class Index : PageModel
{
    public void OnGet()
    {
        
    }
}