﻿using Microsoft.AspNetCore.Mvc;
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
    }

    public class Channel
    {
        public ulong Id { get; set; }
        public string Name { get; set; }

        public Channel(RestTextChannel channel)
        {
            Id = channel.Id;
            Name = channel.Name;
        }

        public Channel(RestVoiceChannel channel)
        {
            Id = channel.Id;
            Name = channel.Name;
        }
    }
}