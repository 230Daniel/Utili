using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Utili.Database;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Utili.Backend.Authorisation;
using Utili.Backend.Models;
using Utili.Backend.Extensions;
using Utili.Backend.Services;

namespace Utili.Backend.Controllers
{
    [Route("premium")]
    public class PremiumController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _databaseContext;
        private readonly IsPremiumService _isPremiumService;

        public PremiumController(
            IMapper mapper,
            DatabaseContext databaseContext,
            IsPremiumService isPremiumService)
        {
            _mapper = mapper;
            _databaseContext = databaseContext;
            _isPremiumService = isPremiumService;
        }

        [HttpGet("free")]
        public IActionResult IsFreePremium()
        {
            return Json(_isPremiumService.IsFree);
        }

        [DiscordGuildAuthorise]
        [HttpGet("guild/{GuildId}")]
        public async Task<IActionResult> GuildIsPremiumAsync([Required] ulong guildId)
        {
            var isPremium = await _isPremiumService.GetIsGuildPremiumAsync(guildId);
            return Json(isPremium);
        }

        [DiscordAuthorise]
        [HttpGet("slots")]
        public async Task<IActionResult> SlotsAsync()
        {
            if (_isPremiumService.IsFree) return NotFound();

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
            if (_isPremiumService.IsFree) return NotFound();

            var user = HttpContext.GetDiscordUser();
            var slots = await _databaseContext.PremiumSlots.GetAllForUserAsync(user.Id);

            foreach (var slot in slots)
            {
                var model = models.FirstOrDefault(x => x.SlotId == slot.SlotId);
                if (model is null) continue;

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
            if (_isPremiumService.IsFree) return NotFound();

            var user = HttpContext.GetDiscordUser();
            var subscriptions = await _databaseContext.Subscriptions.GetAllForUserAsync(user.Id);
            return Json(_mapper.Map<IEnumerable<SubscriptionModel>>(subscriptions));
        }
    }
}
