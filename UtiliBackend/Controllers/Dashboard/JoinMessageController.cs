﻿using System.ComponentModel.DataAnnotations;
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

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
