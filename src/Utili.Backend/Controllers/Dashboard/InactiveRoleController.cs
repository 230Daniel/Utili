using System;
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
using Utili.Backend.Services;

namespace Utili.Backend.Controllers;

[DiscordGuildAuthorise]
[Route("dashboard/{GuildId}/inactive-role")]
public class InactiveRoleController : Controller
{
    private readonly IMapper _mapper;
    private readonly DatabaseContext _dbContext;
    private readonly IsPremiumService _isPremiumService;

    public InactiveRoleController(IMapper mapper, DatabaseContext dbContext, IsPremiumService isPremiumService)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _isPremiumService = isPremiumService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([Required] ulong guildId)
    {
        var configuration = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(guildId);
        configuration ??= new InactiveRoleConfiguration(guildId)
        {
            Threshold = TimeSpan.FromDays(30),
            Mode = InactiveRoleMode.GrantWhenInactive,
            AutoKickThreshold = TimeSpan.FromDays(30),
            DefaultLastAction = DateTime.MinValue,
            LastUpdate = DateTime.MinValue
        };
        return Json(_mapper.Map<InactiveRoleConfigurationModel>(configuration));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] InactiveRoleConfigurationModel model)
    {
        if (model.AutoKick &&
            !await _isPremiumService.GetIsGuildPremiumAsync(guildId))
            model.AutoKick = false;

        var configuration = await _dbContext.InactiveRoleConfigurations.GetForGuildAsync(guildId);

        if (configuration is null)
        {
            configuration = new InactiveRoleConfiguration(guildId);
            model.ApplyTo(configuration);
            configuration.DefaultLastAction = DateTime.UtcNow;
            configuration.LastUpdate = DateTime.UtcNow.AddMinutes(5);
            _dbContext.InactiveRoleConfigurations.Add(configuration);
        }
        else
        {
            if (configuration.RoleId == 0) configuration.DefaultLastAction = DateTime.UtcNow;
            model.ApplyTo(configuration);
            _dbContext.InactiveRoleConfigurations.Update(configuration);
        }

        await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.InactiveRole, configuration.RoleId != 0);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}