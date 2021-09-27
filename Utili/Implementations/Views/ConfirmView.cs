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

        private IUserMessage Message => (Menu as DefaultMenu).Message;

        public ConfirmView(Snowflake memberId, string title, string content, string confirmButtonLabel)
        : base(new LocalMessage().AddEmbed(MessageUtils.CreateEmbed(EmbedType.Info, title, content)))
        {
            var cancelButton = new ButtonViewComponent(async e =>
            {
                if (e.Member.Id != memberId) return;
                Result = false;
                Menu.Stop();
            })
            {
                Label = "Cancel",
                Style = LocalButtonComponentStyle.Secondary
            };

            var confirmButton = new ButtonViewComponent(async e =>
            {
                if (e.Member.Id != memberId) return;
                Result = true;
                Menu.Stop();
            })
            {
                Label = confirmButtonLabel,
                Style = LocalButtonComponentStyle.Danger
            };
            
            AddComponent(cancelButton);
            AddComponent(confirmButton);
        }

        public override async ValueTask DisposeAsync()
        {
            await Message.DeleteAsync();
        }
    }
}
