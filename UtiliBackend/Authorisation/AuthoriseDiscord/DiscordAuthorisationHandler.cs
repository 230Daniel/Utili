using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UtiliBackend.Services;

namespace UtiliBackend.Authorisation
{
    public class DiscordAuthorisationHandler : AuthorizationHandler<DiscordRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordClientService _discordClientService;

        public DiscordAuthorisationHandler(IServiceScopeFactory scopeFactory, DiscordClientService discordClientService)
        {
            _scopeFactory = scopeFactory;
            _discordClientService = discordClientService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DiscordRequirement requirement)
        {
            var identity = context.User.Identities.FirstOrDefault(x => x.AuthenticationType == "Discord");
            
            if (identity is not null && identity.IsAuthenticated)
            {
                var httpContext = (HttpContext) context.Resource;
                var client = await _discordClientService.GetClientAsync(httpContext);
                httpContext.Items["DiscordClient"] ??= client;
                
                if (client is not null)
                {
                    requirement.DiscordAuthenticated = true;
                    context.Succeed(requirement);
                }
                
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == client.CurrentUser.Id);

                if (user is null)
                {
                    user = new User(client.CurrentUser.Id)
                    {
                        Email = client.CurrentUser.Email
                    };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }
                else if (user.Email != client.CurrentUser.Email)
                {
                    user.Email = client.CurrentUser.Email;
                    db.Users.Update(user);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
