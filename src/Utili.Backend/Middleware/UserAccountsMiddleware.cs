using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utili.Database;
using Utili.Database.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utili.Backend.Extensions;

namespace Utili.Backend.Middleware;

public class UserAccountsMiddleware
{
    private readonly RequestDelegate _next;
    private static Dictionary<ulong, SemaphoreSlim> _semaphores = new();

    public UserAccountsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<UserAccountsMiddleware> logger, DatabaseContext db)
    {
        var discordUser = context.GetDiscordUser();
        if (discordUser is not null)
        {
            SemaphoreSlim semaphore;

            lock (_semaphores)
            {
                if (!_semaphores.TryGetValue(discordUser.Id, out semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    _semaphores.Add(discordUser.Id, semaphore);
                }
            }

            await semaphore.WaitAsync();

            try
            {
                var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == discordUser.Id.RawValue);

                if (user is null)
                {
                    user = new User(discordUser.Id)
                    {
                        Email = discordUser.Email
                    };

                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                    logger.LogInformation("Created user for {UserId} with email {Email}", discordUser.Id, user.Email);
                }
                else if (user.Email != discordUser.Email)
                {
                    user.Email = discordUser.Email;
                    db.Users.Update(user);
                    await db.SaveChangesAsync();
                    logger.LogInformation("Updated user {UserId} with new email {Email}", discordUser.Id, user.Email);
                }

                context.Items["User"] = user;
            }
            finally
            {
                semaphore.Release();
            }
        }

        await _next(context);
    }
}