using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages
{
    public class TermsModel : PageModel
    {
        public async Task OnGet()
        {
            await Auth.GetOptionalAuthDetailsAsync(this);
        }
    }
}
