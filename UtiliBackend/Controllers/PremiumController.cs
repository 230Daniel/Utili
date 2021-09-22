using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Database;
using Database.Entities;
using Database.Extensions;
using UtiliBackend.Authorisation;
using UtiliBackend.Extensions;
using UtiliBackend.Models;

namespace UtiliBackend.Controllers
{
    [Route("premium")]
    public class PremiumController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _databaseContext;

        public PremiumController(IMapper mapper, DatabaseContext databaseContext)
        {
            _mapper = mapper;
            _databaseContext = databaseContext;
        }
        
        [DiscordGuildAuthorise]
        [HttpGet("guild/{GuildId}")]
        public async Task<IActionResult> GuildIsPremiumAsync([Required] ulong guildId)
        {
            var isPremium = await _databaseContext.PremiumSlots.AnyAsync(x => x.GuildId == guildId);
            return Json(isPremium);
        }

        [DiscordAuthorise]
        [HttpGet("slots")]
        public async Task<IActionResult> SlotsAsync()
        {
            var user = HttpContext.GetDiscordUser();
            var slots = await _databaseContext.PremiumSlots.GetAllForUserAsync(user.Id);
            var subscriptions = await _databaseContext.Subscriptions.GetValidForUserAsync(user.Id);

            if (slots.Count < subscriptions.Sum(x => x.Slots))
            {
                while (slots.Count < subscriptions.Sum(x => x.Slots))
                {
                    var slot = new PremiumSlot(user.Id);
                    _databaseContext.PremiumSlots.Add(slot);
                    slots.Add(slot);
                }
                await _databaseContext.SaveChangesAsync();
            }
            
            return Json(_mapper.Map<IEnumerable<PremiumSlotModel>>(slots));
        }
        
        [DiscordAuthorise]
        [HttpPost("slots")]
        public async Task<IActionResult> SlotsAsync([FromBody] List<PremiumSlotModel> models)
        {
            var user = HttpContext.GetDiscordUser();
            var slots = await _databaseContext.PremiumSlots.GetAllForUserAsync(user.Id);

            foreach (var slot in slots)
            {
                var model = models.FirstOrDefault(x => x.SlotId == slot.SlotId);
                if(model is null) continue;
                
                model.ApplyTo(slot);
                _databaseContext.PremiumSlots.Update(slot);
            }

            await _databaseContext.SaveChangesAsync();
            return Ok();
        }

        [DiscordAuthorise]
        [HttpGet("subscriptions")]
        public async Task<IActionResult> SubscriptionsAsync()
        {
            var user = HttpContext.GetDiscordUser();
            var subscriptions = await _databaseContext.Subscriptions.GetAllForUserAsync(user.Id);
            return Json(_mapper.Map<IEnumerable<SubscriptionModel>>(subscriptions));
        }
    }
}
