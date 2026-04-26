using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Utili.Database;

public class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Directory.GetCurrentDirectory() + "/../Utili.Bot/appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<DatabaseContext>();
        var connectionString = configuration["Database:Connection"];

        builder.UseNpgsql(connectionString);
        builder.UseSnakeCaseNamingConvention();

        return new DatabaseContext(builder.Options);
    }
}