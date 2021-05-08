using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Utili.Implementations
{
    class MyPagedMenu : PagedMenu
    {
        public MyPagedMenu(Snowflake userId, IPageProvider pageProvider, bool addDefaultButtons = true) : base(userId,
            pageProvider, addDefaultButtons)
        {
            StopBehavior = StopBehavior.ClearReactions;
        }

        protected override async ValueTask<bool> CheckReactionAsync(ButtonEventArgs e)
        {
            if (e.WasAdded)
            {
                await e.Message.RemoveReactionAsync(e.Emoji, e.UserId);
                return false;
            }
            return e.UserId == UserId;
        }
    }
}
