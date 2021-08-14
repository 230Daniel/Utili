using System.Collections.Generic;
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
    [Route("dashboard/{GuildId}/voice-roles")]
    public class VoiceRolesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public VoiceRolesController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configurations = await _dbContext.VoiceRoleConfigurations.GetAllForGuildAsync(guildId);
            return Json(_mapper.Map<IEnumerable<VoiceRoleConfigurationModel>>(configurations));
        }
        
        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] List<VoiceRoleConfigurationModel> models)
        {
            var configurations = await _dbContext.VoiceRoleConfigurations.GetAllForGuildAsync(guildId);
            
            foreach (var model in models)
            {
                var channelId = ulong.Parse(model.ChannelId);
                var configuration = configurations.FirstOrDefault(x => x.ChannelId == channelId);
                
                if (configuration is null)
                {
                    configuration = new VoiceRoleConfiguration(guildId, channelId);
                    model.ApplyTo(configuration);
                    _dbContext.VoiceRoleConfigurations.Add(configuration);
                }
                else
                {
                    model.ApplyTo(configuration);
                    _dbContext.VoiceRoleConfigurations.Update(configuration);
                }
            }

            _dbContext.VoiceRoleConfigurations.RemoveRange(configurations.Where(x => models.All(y => y.ChannelId != x.ChannelId.ToString())));
            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.VoiceRoles, models.Any());
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
