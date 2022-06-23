using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Database;
using Database.Entities;
using Database.Extensions;
using Microsoft.EntityFrameworkCore;
using UtiliBackend.Authorisation;
using UtiliBackend.Extensions;
using UtiliBackend.Models;

namespace UtiliBackend.Controllers
{
    [DiscordGuildAuthorise]
    [Route("dashboard/{GuildId}/role-linking")]
    public class RoleLinkingController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public RoleLinkingController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configurations = await _dbContext.RoleLinkingConfigurations.GetAllForGuildAsync(guildId);
            return Json(_mapper.Map<IEnumerable<RoleLinkingConfigurationModel>>(configurations));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] List<RoleLinkingConfigurationModel> models)
        {
            var configurations = await _dbContext.RoleLinkingConfigurations.GetAllForGuildAsync(guildId);

            if (models.Count > 2)
            {
                var premium = await _dbContext.PremiumSlots.AnyAsync(x => x.GuildId == guildId);
                if (!premium) models = models.OrderBy(x => x.Id).Take(2).ToList();
            }

            foreach (var model in models)
            {
                var configuration = configurations.FirstOrDefault(x => x.Id == model.Id);

                if (configuration is null)
                {
                    configuration = new RoleLinkingConfiguration(guildId);
                    model.ApplyTo(configuration);
                    await _dbContext.RoleLinkingConfigurations.AddAsync(configuration);
                }
                else
                {
                    model.ApplyTo(configuration);
                    _dbContext.RoleLinkingConfigurations.Update(configuration);
                }
            }

            _dbContext.RoleLinkingConfigurations.RemoveRange(configurations.Where(x => models.All(y => y.Id != x.Id)));
            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.RoleLinking, models.Any());
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
