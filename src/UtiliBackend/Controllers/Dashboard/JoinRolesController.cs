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
    [Route("dashboard/{GuildId}/join-roles")]
    public class JoinRolesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public JoinRolesController(IMapper mapper, DatabaseContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configuration = await _dbContext.JoinRolesConfigurations.GetForGuildAsync(guildId);
            configuration ??= new JoinRolesConfiguration(guildId)
            {
                JoinRoles = new()
            };
            return Json(_mapper.Map<JoinRolesConfigurationModel>(configuration));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] JoinRolesConfigurationModel model)
        {
            var configuration = await _dbContext.JoinRolesConfigurations.GetForGuildAsync(guildId);

            if (configuration is null)
            {
                configuration = new JoinRolesConfiguration(guildId);
                model.ApplyTo(configuration);
                _dbContext.JoinRolesConfigurations.Add(configuration);
            }
            else
            {
                model.ApplyTo(configuration);
                _dbContext.JoinRolesConfigurations.Update(configuration);
            }

            await _dbContext.SetHasFeatureAsync(guildId, BotFeatures.JoinRoles, model.JoinRoles.Any());
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
