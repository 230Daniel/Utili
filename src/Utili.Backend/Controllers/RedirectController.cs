using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Utili.Backend.Controllers;

public class RedirectController : Controller
{
    private readonly IConfiguration _configuration;

    public RedirectController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("/")]
    public IActionResult Get()
    {
        return Redirect($"{_configuration["Frontend:Origin"]}");
    }
}