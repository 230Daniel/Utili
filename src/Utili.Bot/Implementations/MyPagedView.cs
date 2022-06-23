using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Utili.Bot.Implementations
{
    public class MyPagedView : PagedViewBase
    {
        public ButtonViewComponent PreviousPageButton { get; }
        public ButtonViewComponent NextPageButton { get; }

        public MyPagedView(PageProvider pageProvider)
            : base(pageProvider)
        {
            PreviousPageButton = new ButtonViewComponent(OnPreviousPageButtonAsync)
            {
                Label = "<─",
                Style = LocalButtonComponentStyle.Secondary
            };
            NextPageButton = new ButtonViewComponent(OnNextPageButtonAsync)
            {
                Label = "─>",
                Style = LocalButtonComponentStyle.Secondary
            };

            AddComponent(PreviousPageButton);
            AddComponent(NextPageButton);
        }

        protected static LocalMessage GetPagelessMessage()
            => new LocalMessage().WithContent("No pages to view.");

        protected void ApplyPageIndex(Page page)
        {
            var indexText = $"Page {CurrentPageIndex + 1} of {PageProvider.PageCount}";
            var embed = page.Embeds.LastOrDefault();
            if (embed != null)
            {
                if (embed.Footer != null)
                {
                    if (embed.Footer.Text == null)
                        embed.Footer.Text = indexText;
                    else if (embed.Footer.Text.Length + indexText.Length + 3 <= LocalEmbedFooter.MaxTextLength)
                        embed.Footer.Text += $" | {indexText}";
                }
                else
                {
                    embed.WithFooter(indexText);
                }
            }
            else
            {
                if (page.Content == null)
                    page.Content = indexText;
                else if (page.Content.Length + indexText.Length + 1 <= LocalMessageBase.MaxContentLength)
                    page.Content += $"\n{indexText}";
            }
        }

        public override async ValueTask UpdateAsync()
        {
            var previousPage = CurrentPage;
            await base.UpdateAsync().ConfigureAwait(false);

            var currentPage = CurrentPage;
            if (currentPage != null)
            {
                var currentPageIndex = CurrentPageIndex;
                var pageCount = PageProvider.PageCount;
                PreviousPageButton.IsDisabled = currentPageIndex == 0;
                NextPageButton.IsDisabled = currentPageIndex == pageCount - 1;

                if (previousPage != currentPage)
                {
                    currentPage = currentPage.Clone();
                    ApplyPageIndex(currentPage);
                    CurrentPage = currentPage;
                }
            }
            else
            {
                TemplateMessage ??= GetPagelessMessage();
                PreviousPageButton.IsDisabled = true;
                NextPageButton.IsDisabled = true;
            }
        }

        protected ValueTask OnPreviousPageButtonAsync(ButtonEventArgs e)
        {
            CurrentPageIndex--;
            return default;
        }

        protected ValueTask OnNextPageButtonAsync(ButtonEventArgs e)
        {
            CurrentPageIndex++;
            return default;
        }
    }
}
