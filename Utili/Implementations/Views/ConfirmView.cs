using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using Utili.Utils;

namespace Utili.Implementations.Views
{
    public class ConfirmView : ViewBase
    {
        public bool Result;
        
        private readonly ConfirmViewOptions _options;
        private IUserMessage Message => (Menu as DefaultMenu).Message;

        public ConfirmView(Snowflake memberId, ConfirmViewOptions options)
        : base(new LocalMessage().AddEmbed(MessageUtils.CreateEmbed(EmbedType.Info, options.PromptTitle, options.PromptDescription)))
        {
            _options = options;
            
            var cancelButton = new ButtonViewComponent(async e =>
            {
                if (e.Member.Id != memberId) return;
                Result = false;
                Menu.Stop();
            })
            {
                Label = _options.PromptCancelButtonLabel,
                Style = LocalButtonComponentStyle.Secondary
            };

            var confirmButton = new ButtonViewComponent(async e =>
            {
                if (e.Member.Id != memberId) return;
                Result = true;
                Menu.Stop();
            })
            {
                Label = _options.PromptConfirmButtonLabel,
                Style = LocalButtonComponentStyle.Danger
            };
            
            AddComponent(cancelButton);
            AddComponent(confirmButton);
        }

        public override async ValueTask DisposeAsync()
        {
            if (Result)
                await Message.ModifyAsync(x =>
                {
                    x.Embeds = new[] {MessageUtils.CreateEmbed(EmbedType.Success, _options.ConfirmTitle, _options.ConfirmDescription)};
                    x.Components = new LocalRowComponent[]{ };
                });
            else
                await Message.ModifyAsync(x =>
                {
                    x.Embeds = new[] {MessageUtils.CreateEmbed(EmbedType.Failure, _options.CancelTitle, _options.CancelDescription)};
                    x.Components = new LocalRowComponent[]{ };
                });
        }
    }

    public class ConfirmViewOptions
    {
        public string PromptTitle { get; set; } = "Are you sure?";
        public string PromptDescription { get; set; }
        public string PromptCancelButtonLabel { get; set; } = "Cancel";
        public string PromptConfirmButtonLabel { get; set; } = "Confirm";
        public string CancelTitle { get; set; } = "Operation canceled";
        public string CancelDescription { get; set; }
        public string ConfirmTitle { get; set; }
        public string ConfirmDescription { get; set; }
    }
}
