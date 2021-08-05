using System;
using Microsoft.Extensions.DependencyInjection;
using NewDatabase;

namespace Utili.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static DatabaseContext GetDatabaseContext(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<DatabaseContext>();
        }
    }
}
