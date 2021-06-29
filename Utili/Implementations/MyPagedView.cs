using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Utili.Implementations
{
    public class MyPagedView : PagedViewBase
    {
        public MyPagedView(IPageProvider pageProvider) : base(pageProvider) { }

        [Button(Label = "<─", Style = ButtonComponentStyle.Primary)]
        public ValueTask Previous(ButtonEventArgs e)
        {
            CurrentPageIndex--;
            return default;
        }

        [Button(Label = "─>", Style = ButtonComponentStyle.Primary)]
        public ValueTask Next(ButtonEventArgs e)
        {
            CurrentPageIndex++;
            return default;
        }
    }
}
