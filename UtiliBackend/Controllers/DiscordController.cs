using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UtiliBackend.Authorisation;
using UtiliBackend.Extensions;
using UtiliBackend.Models;
using UtiliBackend.Services;

namespace UtiliBackend.Controllers
{
    [DiscordGuildAuthorise]
    [Route("discord/{GuildId}")]
    public class DiscordController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DiscordRestService _discordRestService;

        public DiscordController(IMapper mapper, DiscordRestService discordRestService)
        {
            _mapper = mapper;
            _discordRestService = discordRestService;
        }
        
        [HttpGet("text-channels")]
        public async Task<IActionResult> TextChannelsAsync([Required] ulong guildId)
        {
            var guild = HttpContext.GetDiscordGuild();
            var channels = await _discordRestService.GetTextChannelsAsync(guild);
            return Json(_mapper.Map<IEnumerable<TextChannelModel>>(channels));
        }
        
        [HttpGet("voice-channels")]
        public async Task<IActionResult> VoiceChannelsAsync([Required] ulong guildId)
        {
            var guild = HttpContext.GetDiscordGuild();
            var channels = await _discordRestService.GetVoiceChannelsAsync(guild);
            return Json(_mapper.Map<IEnumerable<VoiceChannelModel>>(channels));
        }
        
        [HttpGet("roles")]
        public IActionResult Roles([Required] ulong guildId)
        {
            var guild = HttpContext.GetDiscordGuild();
            var roles = guild.Roles.Where(x => !x.IsManaged && !x.IsEveryone);
            return Json(_mapper.Map<IEnumerable<RoleModel>>(roles));
        }
    }
}
