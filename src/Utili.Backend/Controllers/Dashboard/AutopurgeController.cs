﻿using System.Collections.Generic;
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
[Route("dashboard/{GuildId}/autopurge")]
public class AutopurgeController : Controller
{
    private readonly IMapper _mapper;
    private readonly DatabaseContext _dbContext;

    public AutopurgeController(IMapper mapper, DatabaseContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([Required] ulong guildId)
    {
        var configurations = await _dbContext.AutopurgeConfigurations.GetAllForGuildAsync(guildId);
        return Json(_mapper.Map<IEnumerable<AutopurgeConfigurationModel>>(configurations));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] List<AutopurgeConfigurationModel> models)
    {
        var configurations = await _dbContext.AutopurgeConfigurations.GetAllForGuildAsync(guildId);

        foreach (var model in models)
        {
            var channelId = ulong.Parse(model.ChannelId);
            var configuration = configurations.FirstOrDefault(x => x.ChannelId == channelId);

            if (configuration is null)
            {
                configuration = new AutopurgeConfiguration(guildId, channelId);
                model.ApplyTo(configuration);
                configuration.AddedFromDashboard = true;
                _dbContext.AutopurgeConfigurations.Add(configuration);
            }
            else
            {
                model.ApplyTo(configuration);
                _dbContext.AutopurgeConfigurations.Update(configuration);
            }
        }

        _dbContext.AutopurgeConfigurations.RemoveRange(configurations.Where(x => models.All(y => y.ChannelId != x.ChannelId.ToString())));
        await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.Autopurge, models.Any());
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}