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
    [Route("discord")]
    public class DiscordController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DiscordRestService _discordRestService;
        private readonly DiscordUserGuildsService _discordUserGuildsService;

        public DiscordController(IMapper mapper, DiscordRestService discordRestService, DiscordUserGuildsService discordUserGuildsService)
        {
            _mapper = mapper;
            _discordRestService = discordRestService;
            _discordUserGuildsService = discordUserGuildsService;
        }

        [DiscordAuthorise]
        [HttpGet("guilds")]
        public async Task<IActionResult> GuildsAsync()
        {
            var guilds = await _discordUserGuildsService.GetGuildsAsync(HttpContext);
            return Json(guilds.Select(guild => new GuildModel
            {
                Id = guild.Id.ToString(),
                Name = guild.Name,
                IsManageable = guild.Permissions.ManageGuild,
                IconUrl = string.IsNullOrEmpty(guild.IconUrl)
                    ? "https://cdn.discordapp.com/embed/avatars/1.png" 
                    : guild.IconUrl.Remove(guild.IconUrl.Length - 4) + ".png?size=256"
            }));
        }
        
        [DiscordGuildAuthorise]
        [HttpGet("{GuildId}/text-channels")]
        public async Task<IActionResult> TextChannelsAsync([Required] ulong guildId)
        {
            var channels = await _discordRestService.GetTextChannelsAsync(guildId);
            return Json(_mapper.Map<IEnumerable<TextChannelModel>>(channels));
        }
        
        [DiscordGuildAuthorise]
        [HttpGet("{GuildId}/vocal-channels")]
        public async Task<IActionResult> VocalChannelsAsync([Required] ulong guildId)
        {
            var channels = await _discordRestService.GetVocalChannelsAsync(guildId);
            return Json(_mapper.Map<IEnumerable<VocalChannelModel>>(channels));
        }
        
        [DiscordGuildAuthorise]
        [HttpGet("{GuildId}/roles")]
        public IActionResult Roles([Required] ulong guildId)
        {
            var guild = HttpContext.GetDiscordGuild();
            var roles = guild.Roles.Values.Where(x => !x.IsManaged && x.Id != guildId);
            return Json(_mapper.Map<IEnumerable<RoleModel>>(roles));
        }
    }
}
