using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Utili.Database;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Utili.Backend.Authorisation;
using Utili.Backend.Models;
using Utili.Backend.Extensions;

namespace Utili.Backend.Controllers
{
    [DiscordGuildAuthorise]
    [Route("dashboard/{GuildId}/voice-link")]
    public class VoiceLinkController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public VoiceLinkController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configuration = await _dbContext.VoiceLinkConfigurations.GetForGuildAsync(guildId);
            configuration ??= new VoiceLinkConfiguration(guildId)
            {
                Enabled = false,
                DeleteChannels = true,
                ChannelPrefix = "vc-"
            };
            configuration.ChannelPrefix ??= "vc-";
            return Json(_mapper.Map<VoiceLinkConfigurationModel>(configuration));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] VoiceLinkConfigurationModel model)
        {
            var configuration = await _dbContext.VoiceLinkConfigurations.GetForGuildAsync(guildId);

            if (configuration is null)
            {
                configuration = new VoiceLinkConfiguration(guildId);
                model.ApplyTo(configuration);
                _dbContext.VoiceLinkConfigurations.Add(configuration);
            }
            else
            {
                model.ApplyTo(configuration);
                _dbContext.VoiceLinkConfigurations.Update(configuration);
            }

            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.VoiceLink, model.Enabled);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
