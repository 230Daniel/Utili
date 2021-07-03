using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewDatabase.Entities;
using NewDatabase.Extensions;

namespace NewDatabase
{
    public class DatabaseContext : DbContext
    {
        public DbSet<AutopurgeConfiguration> AutopurgeConfigurations { get; set; }
        public DbSet<AutopurgeMessage> AutopurgeMessages { get; set; }
        public DbSet<CoreConfiguration> CoreConfigurations { get; set; }
        public DbSet<InactiveRoleConfiguration> InactiveRoleConfigurations { get; set; }
        
        private readonly string _connectionString;
        
        public DatabaseContext(IConfiguration configuration)
        {
            _connectionString = configuration["Database:Connection"];
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(_connectionString);
            options.UseSnakeCaseNamingConvention();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureGuildEntities();
            modelBuilder.ConfigureGuildChannelEntities();
            modelBuilder.ConfigureMessageEntities();
            modelBuilder.ConfigureOtherEntities();
            modelBuilder.ConfigureUlongListConverters();
        }
    }
}
