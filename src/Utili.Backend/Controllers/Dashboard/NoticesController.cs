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

namespace Utili.Backend.Controllers;

[DiscordGuildAuthorise]
[Route("dashboard/{GuildId}/notices")]
public class NoticesController : Controller
{
    private readonly IMapper _mapper;
    private readonly DatabaseContext _dbContext;

    public NoticesController(IMapper mapper, DatabaseContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([Required] ulong guildId)
    {
        var configurations = await _dbContext.NoticeConfigurations.GetAllForGuildAsync(guildId);
        return Json(_mapper.Map<IEnumerable<NoticeConfigurationModel>>(configurations));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] List<NoticeConfigurationModel> models)
    {
        var configurations = await _dbContext.NoticeConfigurations.GetAllForGuildAsync(guildId);

        foreach (var model in models)
        {
            var channelId = ulong.Parse(model.ChannelId);
            var configuration = configurations.FirstOrDefault(x => x.ChannelId == channelId);

            if (configuration is null)
            {
                configuration = new NoticeConfiguration(guildId, channelId);
                model.ApplyTo(configuration);
                _dbContext.NoticeConfigurations.Add(configuration);
            }
            else
            {
                model.ApplyTo(configuration);
                _dbContext.NoticeConfigurations.Update(configuration);
            }
        }

        _dbContext.NoticeConfigurations.RemoveRange(configurations.Where(x => models.All(y => y.ChannelId != x.ChannelId.ToString())));
        await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.Notices, models.Any());
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}