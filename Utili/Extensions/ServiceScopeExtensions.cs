using Microsoft.Extensions.DependencyInjection;
using NewDatabase;

namespace Utili.Extensions
{
    public static class ServiceScopeExtensions
    {
        public static DatabaseContext GetDbContext(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetDbContext();
        }
    }
}
