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
[Route("dashboard/{GuildId}/message-pinning")]
public class MessagePinningController : Controller
{
    private readonly IMapper _mapper;
    private readonly DatabaseContext _dbContext;

    public MessagePinningController(IMapper mapper, DatabaseContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([Required] ulong guildId)
    {
        var configuration = await _dbContext.MessagePinningConfigurations.GetForGuildAsync(guildId);
        configuration ??= new MessagePinningConfiguration(guildId);
        return Json(_mapper.Map<MessagePinningConfigurationModel>(configuration));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] MessagePinningConfigurationModel model)
    {
        var configuration = await _dbContext.MessagePinningConfigurations.GetForGuildAsync(guildId);

        if (configuration is null)
        {
            configuration = new MessagePinningConfiguration(guildId);
            model.ApplyTo(configuration);
            _dbContext.MessagePinningConfigurations.Add(configuration);
        }
        else
        {
            model.ApplyTo(configuration);
            _dbContext.MessagePinningConfigurations.Update(configuration);
        }

        await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.MessagePinning, true);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}