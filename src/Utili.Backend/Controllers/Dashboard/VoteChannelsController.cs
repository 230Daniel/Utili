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
    [DiscordGuildAuthorise]
    [Route("dashboard/{GuildId}/vote-channels")]
    public class VoteChannelsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;
        private readonly IsPremiumService _isPremiumService;

        public VoteChannelsController(IMapper mapper, DatabaseContext dbContext, IsPremiumService isPremiumService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _isPremiumService = isPremiumService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configurations = await _dbContext.VoteChannelConfigurations.GetAllForGuildAsync(guildId);
            return Json(_mapper.Map<IEnumerable<VoteChannelConfigurationModel>>(configurations));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] List<VoteChannelConfigurationModel> models)
        {
            var configurations = await _dbContext.VoteChannelConfigurations.GetAllForGuildAsync(guildId);

            if (models.Any(x => x.Emojis.Count > 2))
            {
                var premium = await _isPremiumService.GetIsGuildPremiumAsync(guildId);
                var amountOfEmojis = premium ? 5 : 2;
                foreach (var model in models)
                {
                    model.Emojis = model.Emojis.Take(amountOfEmojis).ToList();
                }
            }

            foreach (var model in models)
            {
                var channelId = ulong.Parse(model.ChannelId);
                var configuration = configurations.FirstOrDefault(x => x.ChannelId == channelId);

                if (configuration is null)
                {
                    configuration = new VoteChannelConfiguration(guildId, channelId);
                    model.ApplyTo(configuration);
                    _dbContext.VoteChannelConfigurations.Add(configuration);
                }
                else
                {
                    model.ApplyTo(configuration);
                    _dbContext.VoteChannelConfigurations.Update(configuration);
                }
            }

            _dbContext.VoteChannelConfigurations.RemoveRange(configurations.Where(x => models.All(y => y.ChannelId != x.ChannelId.ToString())));
            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.VoteChannels, models.Any());
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
