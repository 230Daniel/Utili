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

namespace Utili.Backend.Controllers;

[DiscordGuildAuthorise]
[Route("dashboard/{GuildId}/join-message")]
public class JoinMessageController : Controller
{
    private readonly IMapper _mapper;
    private readonly DatabaseContext _dbContext;

    public JoinMessageController(IMapper mapper, DatabaseContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([Required] ulong guildId)
    {
        var configuration = await _dbContext.JoinMessageConfigurations.GetForGuildAsync(guildId);
        configuration ??= new JoinMessageConfiguration(guildId)
        {
            ThreadTitle = "Welcome %user%",
            Title = "",
            Footer = "",
            Content = "",
            Text = "",
            Image = "",
            Thumbnail = "",
            Icon = "",
            Colour = 4437377
        };
        return Json(_mapper.Map<JoinMessageConfigurationModel>(configuration));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] JoinMessageConfigurationModel model)
    {
        var configuration = await _dbContext.JoinMessageConfigurations.GetForGuildAsync(guildId);

        if (configuration is null)
        {
            configuration = new JoinMessageConfiguration(guildId);
            model.ApplyTo(configuration);
            _dbContext.JoinMessageConfigurations.Add(configuration);
        }
        else
        {
            model.ApplyTo(configuration);
            _dbContext.JoinMessageConfigurations.Update(configuration);
        }

        await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.JoinMessage, model.Enabled);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}