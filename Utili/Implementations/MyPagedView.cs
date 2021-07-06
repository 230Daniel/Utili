using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Utili.Implementations
{
    public class MyPagedView : PagedView
    {
        protected override ButtonViewComponent FirstPageButton { get; set; } = null;
        
        public MyPagedView(PageProvider pageProvider) : base(pageProvider)
        {
        }
    }
}
