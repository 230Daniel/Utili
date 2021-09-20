using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UtiliBackend.Middleware
{
    public class AlwaysHttpsMiddleware
    {
        private readonly RequestDelegate _next;
        
        public AlwaysHttpsMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public Task InvokeAsync(HttpContext context)
        {
            context.Request.Scheme = "https";
            return _next(context);
        }
    }
}
