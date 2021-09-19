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
    [DiscordGuildAuthorise]
    [Route("dashboard/{GuildId}/message-filter")]
    public class MessageFilterController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public MessageFilterController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configurations = await _dbContext.MessageFilterConfigurations.GetAllForGuildAsync(guildId);
            foreach (var configuration in configurations)
            {
                configuration.RegEx ??= "";
                configuration.DeletionMessage ??= "";
            }
            return Json(_mapper.Map<IEnumerable<MessageFilterConfigurationModel>>(configurations));
        }
        
        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] List<MessageFilterConfigurationModel> models)
        {
            var configurations = await _dbContext.MessageFilterConfigurations.GetAllForGuildAsync(guildId);
            
            foreach (var model in models)
            {
                var channelId = ulong.Parse(model.ChannelId);
                var configuration = configurations.FirstOrDefault(x => x.ChannelId == channelId);
                
                if (configuration is null)
                {
                    configuration = new MessageFilterConfiguration(guildId, channelId);
                    model.ApplyTo(configuration);
                    _dbContext.MessageFilterConfigurations.Add(configuration);
                }
                else
                {
                    model.ApplyTo(configuration);
                    _dbContext.MessageFilterConfigurations.Update(configuration);
                }
            }

            _dbContext.MessageFilterConfigurations.RemoveRange(configurations.Where(x => models.All(y => y.ChannelId != x.ChannelId.ToString())));
            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.MessageFilter, models.Any());
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
