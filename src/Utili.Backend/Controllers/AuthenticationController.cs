﻿using Disqord;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Utili.Backend.Authorisation;
using Utili.Backend.Models;
using Utili.Backend.Extensions;

namespace Utili.Backend.Controllers;

[Route("authentication")]
public class AuthenticationController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IAntiforgery _antiforgery;

    public AuthenticationController(IConfiguration configuration, IAntiforgery antiforgery)
    {
        _configuration = configuration;
        _antiforgery = antiforgery;
    }

    [HttpGet("signin")]
    public IActionResult SignIn()
    {
        AuthenticationProperties authProperties = new()
        {
            AllowRefresh = true,
            IsPersistent = true,
            RedirectUri = $"{_configuration["Frontend:Origin"]}/return"
        };

        return Challenge(authProperties, "Discord");
    }

    [HttpPost("signout")]
    public new IActionResult SignOut()
    {
        return SignOut("Cookies");
    }

    [DiscordAuthorise]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var user = HttpContext.GetDiscordUser();
        return Json(new AuthenticationInfoModel
        {
            Username = user.HasMigratedName() ? user.GlobalName : user.Name,
            AvatarUrl = user.GetAvatarUrl()
        });
    }

    [IgnoreAntiforgeryToken]
    [HttpGet("antiforgery")]
    public IActionResult Antiforgery()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Json(tokens.RequestToken);
    }
}
