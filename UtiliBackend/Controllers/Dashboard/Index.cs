using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;

namespace UtiliBackend.Controllers.Dashboard
{
    public class Index : Controller
    {
        [HttpGet("dashboard/guilds")]
        public async Task<IActionResult> Guilds()
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext);
            if (!auth.Authorised) return auth.Action;

            List<RestUserGuild> guilds = await DiscordModule.GetManageableGuildsAsync(auth.Client);
            List<RestUserGuild> mutualGuilds = await DiscordModule.GetMutualGuildsAsync(auth.Client);
            return new JsonResult(guilds.Select(x => new PartialGuild(x, mutualGuilds.Any(y => y.Id == x.Id))));
        }
    }

    public class PartialGuild
    {
        public string Id { get; }
        public bool Mutual { get; }
        public string DashboardUrl { get; }
        public string Name { get; }
        public string IconUrl { get; }

        public PartialGuild(RestUserGuild guild, bool mutual)
        {
            Id = guild.Id.ToString();
            Mutual = mutual;
            DashboardUrl = mutual ? $"/dashboard/{guild.Id}" : $"/invite/{guild.Id}";
            Name = guild.Name;
            IconUrl = string.IsNullOrEmpty(guild.IconUrl) ? "https://cdn.discordapp.com/embed/avatars/0.png" : guild.IconUrl.Remove(guild.IconUrl.Length - 4) + ".png?size=256";
        }
    }
}
