using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages
{
    public class PrivacyModel : PageModel
    {
        public async Task OnGet()
        {
            await Auth.GetOptionalAuthDetailsAsync(this);
        }
    }
}
