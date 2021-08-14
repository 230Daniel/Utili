using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewDatabase;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using UtiliBackend.Authorisation;
using UtiliBackend.Extensions;
using UtiliBackend.Models;

namespace UtiliBackend.Controllers
{
    [DiscordGuildAuthorise]
    [Route("dashboard/{GuildId}/reputation")]
    public class ReputationController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public ReputationController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configuration = await _dbContext.ReputationConfigurations.GetForGuildWithEmojisAsync(guildId);
            configuration ??= new ReputationConfiguration(guildId);
            return Json(_mapper.Map<ReputationConfigurationModel>(configuration));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] ReputationConfigurationModel model)
        {
            var configuration = await _dbContext.ReputationConfigurations.GetForGuildWithEmojisAsync(guildId);
            
            if (configuration is null)
            {
                configuration = new ReputationConfiguration(guildId);
                model.ApplyTo(configuration);
                _dbContext.ReputationConfigurations.Add(configuration);
            }
            else
            {
                model.ApplyTo(configuration);
                _dbContext.ReputationConfigurations.Update(configuration);
            }
            
            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.Reputation, model.Emojis.Any());
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
