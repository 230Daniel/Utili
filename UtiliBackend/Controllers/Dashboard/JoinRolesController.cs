using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewDatabase;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using UtiliBackend.Authorisation;
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
                _dbContext.JoinRolesConfigurations.Add(configuration);
                await _dbContext.SaveChangesAsync();
            }

            configuration.WaitForVerification = model.WaitForVerification;
            configuration.JoinRoles = model.JoinRoles.Select(ulong.Parse).ToList();

            _dbContext.JoinRolesConfigurations.Update(configuration);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
