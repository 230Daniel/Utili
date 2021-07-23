using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewDatabase;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using UtiliBackend.Authorisation;
using UtiliBackend.Models;

namespace UtiliBackend.Controllers
{
    [DiscordGuildAuthorise]
    [Route("dashboard/{GuildId}/inactive-role")]
    public class InactiveRoleController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public InactiveRoleController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configuration = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(guildId);
            configuration ??= new InactiveRoleConfiguration(guildId)
            {
                Threshold = TimeSpan.FromDays(30),
                Mode = InactiveRoleMode.GrantWhenInactive,
                AutoKickThreshold = TimeSpan.FromDays(30),
                DefaultLastAction = DateTime.MinValue,
                LastUpdate = DateTime.MinValue,
            };
            return Json(_mapper.Map<InactiveRoleConfigurationModel>(configuration));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] InactiveRoleConfigurationModel model)
        {
            if (model.AutoKick)
            {
                var premium = await _dbContext.PremiumSlots.AnyAsync(x => x.GuildId == guildId);
                if (!premium) model.AutoKick = false;
            }
            
            var configuration = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(guildId);
            
            if (configuration is null)
            {
                configuration = new InactiveRoleConfiguration(guildId);
                model.ApplyTo(configuration);
                _dbContext.InactiveRoleConfigurations.Add(configuration);
            }
            else
            {
                model.ApplyTo(configuration);
                _dbContext.InactiveRoleConfigurations.Update(configuration);
            }
            
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
