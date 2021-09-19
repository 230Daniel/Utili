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
    [Route("dashboard/{GuildId}/message-logs")]
    public class MessageLogsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public MessageLogsController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configuration = await _dbContext.MessageLogsConfigurations.GetForGuildAsync(guildId);
            configuration ??= new MessageLogsConfiguration(guildId)
            {
                ExcludedChannels = new()
            };
            return Json(_mapper.Map<MessageLogsConfigurationModel>(configuration));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] MessageLogsConfigurationModel model)
        {
            var configuration = await _dbContext.MessageLogsConfigurations.GetForGuildAsync(guildId);
            
            if (configuration is null)
            {
                configuration = new MessageLogsConfiguration(guildId);
                model.ApplyTo(configuration);
                _dbContext.MessageLogsConfigurations.Add(configuration);
            }
            else
            {
                model.ApplyTo(configuration);
                _dbContext.MessageLogsConfigurations.Update(configuration);
            }
            
            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.MessageLogs, configuration.DeletedChannelId != 0 || configuration.EditedChannelId != 0);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
