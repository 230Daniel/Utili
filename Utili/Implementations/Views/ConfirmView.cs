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

        private IUserMessage Message => (Menu as InteractiveMenu).Message;
        private bool _clicked;

        public ConfirmView(Snowflake memberId, string title, string content, string confirmButtonLabel)
        : base(new LocalMessage().AddEmbed(MessageUtils.CreateEmbed(EmbedType.Info, title, content)))
        {
            var cancelButton = new ButtonViewComponent(async e =>
            {
                if (e.Member.Id != memberId) return;
                _clicked = true;
                Result = false;
                await Message.DeleteAsync();
                await Menu.StopAsync();
            })
            {
                Label = "Cancel",
                Style = ButtonComponentStyle.Secondary
            };

            var confirmButton = new ButtonViewComponent(async e =>
            {
                if (e.Member.Id != memberId) return;
                _clicked = true;
                Result = true;
                await Message.DeleteAsync();
                await Menu.StopAsync();
            })
            {
                Label = confirmButtonLabel,
                Style = ButtonComponentStyle.Danger
            };
            
            AddComponent(cancelButton);
            AddComponent(confirmButton);

            _ = Task.Run(async () =>
            {
                await Task.Delay(60000);
                if (!_clicked)
                {
                    _clicked = true;
                    Result = false;
                    await Message.DeleteAsync();
                    await Menu.StopAsync();
                }
            });
        }
    }
}
