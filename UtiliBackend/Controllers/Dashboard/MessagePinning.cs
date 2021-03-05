using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Database.Data;

namespace UtiliBackend.Controllers.Dashboard
{
    public class MessagePinning : Controller
    {
        [HttpGet("dashboard/{guildId}/messagepinning")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            MessagePinningRow row = await Database.Data.MessagePinning.GetRowAsync(auth.Guild.Id);
            return new JsonResult(new MessagePinningBody(row));
        }

        [HttpPost("dashboard/{guildId}/messagepinning")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] MessagePinningBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            MessagePinningRow row = await Database.Data.MessagePinning.GetRowAsync(auth.Guild.Id);
            row.Pin = body.Pin;
            row.PinChannelId = ulong.Parse(body.PinChannelId);
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class MessagePinningBody
    {
        public bool Pin { get; set; }
        public string PinChannelId { get; set; }

        public MessagePinningBody(MessagePinningRow row)
        {
            Pin = row.Pin;
            PinChannelId = row.PinChannelId.ToString();
        }

        public MessagePinningBody() { }
    }
}