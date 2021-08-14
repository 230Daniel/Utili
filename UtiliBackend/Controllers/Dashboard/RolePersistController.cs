using System.ComponentModel.DataAnnotations;
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
    [Route("dashboard/{GuildId}/role-persist")]
    public class RolePersistController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public RolePersistController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configuration = await _dbContext.RolePersistConfigurations.GetForGuildAsync(guildId);
            configuration ??= new RolePersistConfiguration(guildId)
            {
                Enabled = false,
                ExcludedRoles = new()
            };
            return Json(_mapper.Map<RolePersistConfigurationModel>(configuration));
        }
        
        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] RolePersistConfigurationModel model)
        {
            var configuration = await _dbContext.RolePersistConfigurations.GetForGuildAsync(guildId);
            
            if (configuration is null)
            {
                configuration = new RolePersistConfiguration(guildId);
                model.ApplyTo(configuration);
                _dbContext.RolePersistConfigurations.Add(configuration);
            }
            else
            {
                model.ApplyTo(configuration);
                _dbContext.RolePersistConfigurations.Update(configuration);
            }
            
            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.RolePersist, model.Enabled);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
