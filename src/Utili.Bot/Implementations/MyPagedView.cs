using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Utili.Bot.Implementations;

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

    protected void ApplyPageIndex(Page page)
    {
        var indexText = $"Page {CurrentPageIndex + 1} of {PageProvider.PageCount}";
        var embed = page.Embeds.HasValue ?
            page.Embeds.Value.LastOrDefault() :
            null;

        if (embed != null)
        {
            if (!embed.Footer.HasValue ||
                !embed.Footer.Value.Text.HasValue ||
                string.IsNullOrWhiteSpace(embed.Footer.Value.Text.Value))
            {
                embed.WithFooter(indexText);
            }
            else if (embed.Footer.Value.Text.Value.Length + indexText.Length + 3 <= Discord.Limits.Message.Embed.Footer.MaxTextLength)
            {
                embed.Footer.Value.Text += $" | {indexText}";
            }
        }
        else
        {
            if (!page.Content.HasValue || string.IsNullOrWhiteSpace(page.Content.Value))
            {
                page.WithContent(indexText);
            }
            else if (page.Content.Value.Length + indexText.Length + 1 <= Discord.Limits.Message.MaxContentLength)
            {
                page.Content += $"\n{indexText}";
            }
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
            MessageTemplate = message => message.WithContent("No pages to view.");
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
