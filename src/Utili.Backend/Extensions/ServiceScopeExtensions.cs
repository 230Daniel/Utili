using Microsoft.Extensions.DependencyInjection;
using Utili.Database;

namespace Utili.Backend.Extensions
{
    public static class ServiceScopeExtensions
    {
        public static DatabaseContext GetDbContext(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetDbContext();
        }
    }
}
