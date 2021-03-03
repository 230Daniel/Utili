using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;

namespace UtiliBackend.Controllers
{
    public class Discord : Controller
    {
        [HttpGet("discord/{guildId}/channels")]
        public async Task<ActionResult> Channels([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<Channel> channels = (await DiscordModule.GetTextChannelsAsync(auth.Guild)).Select(x => new Channel(x)).ToList();
            channels.AddRange((await DiscordModule.GetVoiceChannelsAsync(auth.Guild)).Select(x => new Channel(x)));
            return new JsonResult(channels);
        }

        [HttpGet("discord/{guildId}/channels/text")]
        public async Task<ActionResult> TextChannels([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            return new JsonResult(channels.Select(x => new Channel(x)));
        }

        [HttpGet("discord/{guildId}/channels/voice")]
        public async Task<ActionResult> VoiceChannels([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<RestVoiceChannel> channels = await DiscordModule.GetVoiceChannelsAsync(auth.Guild);
            return new JsonResult(channels.Select(x => new Channel(x)));
        }

        [HttpGet("discord/{guildId}/roles")]
        public async Task<ActionResult> Roles([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<Role> roles = auth.Guild.Roles.Where(x => !x.IsEveryone && !x.IsManaged).OrderBy(x => -x.Position).Select(x => new Role(x)).ToList();
            return new JsonResult(roles);
        }
    }

    public class Channel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Channel(RestTextChannel channel)
        {
            Id = channel.Id.ToString();
            Name = channel.Name;
        }

        public Channel(RestVoiceChannel channel)
        {
            Id = channel.Id.ToString();
            Name = channel.Name;
        }
    }

    public class Role
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Role(RestRole role)
        {
            Id = role.Id.ToString();
            Name = role.Name;
        }
    }
}
