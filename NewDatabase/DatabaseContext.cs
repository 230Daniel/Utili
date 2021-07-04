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
        public DbSet<JoinMessageConfiguration> JoinMessageConfigurations { get; set; }
        public DbSet<JoinRolesConfiguration> JoinRolesConfigurations { get; set; }
        public DbSet<JoinRolesPendingMember> JoinRolesPendingMembers { get; set; }
        public DbSet<MessageFilterConfiguration> MessageFilterConfigurations { get; set; }

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
            modelBuilder.ConfigureMemberEntities();
            modelBuilder.ConfigureMessageEntities();
            modelBuilder.ConfigureOtherEntities();
            modelBuilder.ConfigureUlongListConverters();
        }
    }
}
