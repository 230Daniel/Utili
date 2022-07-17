using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Utili.Backend.Middleware;

public class NoCacheMiddleware
{
    private readonly RequestDelegate _next;

    public NoCacheMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add("cache-control", "no-cache");
        return _next(context);
    }
}