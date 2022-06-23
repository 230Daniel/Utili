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
    [Route("dashboard/{GuildId}/channel-mirroring")]
    public class ChannelMirroringController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public ChannelMirroringController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configurations = await _dbContext.ChannelMirroringConfigurations.GetAllForGuildAsync(guildId);
            return Json(_mapper.Map<IEnumerable<ChannelMirroringConfigurationModel>>(configurations));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] List<ChannelMirroringConfigurationModel> models)
        {
            var configurations = await _dbContext.ChannelMirroringConfigurations.GetAllForGuildAsync(guildId);

            foreach (var model in models)
            {
                var channelId = ulong.Parse(model.ChannelId);
                var configuration = configurations.FirstOrDefault(x => x.ChannelId == channelId);

                if (configuration is null)
                {
                    configuration = new ChannelMirroringConfiguration(guildId, channelId);
                    model.ApplyTo(configuration);
                    _dbContext.ChannelMirroringConfigurations.Add(configuration);
                }
                else
                {
                    model.ApplyTo(configuration);
                    _dbContext.ChannelMirroringConfigurations.Update(configuration);
                }
            }

            _dbContext.ChannelMirroringConfigurations.RemoveRange(configurations.Where(x => models.All(y => y.ChannelId != x.ChannelId.ToString())));
            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.ChannelMirroring, models.Any());
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
