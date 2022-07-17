using System;
using Microsoft.Extensions.DependencyInjection;
using Utili.Database;

namespace Utili.Backend.Extensions;

public static class ServiceProviderExtensions
{
    public static DatabaseContext GetDbContext(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<DatabaseContext>();
    }
}