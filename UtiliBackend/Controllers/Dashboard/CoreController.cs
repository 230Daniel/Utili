using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NewDatabase;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using UtiliBackend.Authorisation;
using UtiliBackend.Models;

namespace UtiliBackend.Controllers
{
    [DiscordGuildAuthorise]
    [Route("dashboard/{GuildId}/core")]
    public class CoreController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public CoreController(IConfiguration configuration, IMapper mapper, DatabaseContext dbContext)
        {
            _configuration = configuration;
            _mapper = mapper;
            _dbContext = dbContext;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAsync([Required] ulong guildId)
        {
            var configuration = await _dbContext.CoreConfigurations.GetForGuildAsync(guildId);
            configuration ??= new CoreConfiguration(guildId)
            {
                Prefix = _configuration["Other:DefaultPrefix"],
                CommandsEnabled = true,
                NonCommandChannels = new()
            };
            return Json(_mapper.Map<CoreConfigurationModel>(configuration));
        }
        
        [HttpPost]
        public async Task<IActionResult> PostAsync([Required] ulong guildId, [FromBody] CoreConfigurationModel model)
        {
            var configuration = await _dbContext.CoreConfigurations.GetForGuildAsync(guildId);
            
            if (configuration is null)
            {
                configuration = new CoreConfiguration(guildId);
                model.ApplyTo(configuration);
                _dbContext.CoreConfigurations.Add(configuration);
            }
            else
            {
                model.ApplyTo(configuration);
                _dbContext.CoreConfigurations.Update(configuration);
            }
            
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
