using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Database.Extensions;

namespace Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<AutopurgeConfiguration> AutopurgeConfigurations { get; internal set; }
        public DbSet<AutopurgeMessage> AutopurgeMessages { get; internal set; }
        public DbSet<ChannelMirroringConfiguration> ChannelMirroringConfigurations { get; set; }
        public DbSet<CoreConfiguration> CoreConfigurations { get; internal set; }
        public DbSet<CustomerDetails> CustomerDetails { get; internal set; }
        public DbSet<InactiveRoleConfiguration> InactiveRoleConfigurations { get; internal set; }
        public DbSet<InactiveRoleMember> InactiveRoleMembers { get; internal set; }
        public DbSet<JoinMessageConfiguration> JoinMessageConfigurations { get; internal set; }
        public DbSet<JoinRolesConfiguration> JoinRolesConfigurations { get; internal set; }
        public DbSet<JoinRolesPendingMember> JoinRolesPendingMembers { get; internal set; }
        public DbSet<MessageFilterConfiguration> MessageFilterConfigurations { get; internal set; }
        public DbSet<MessageLogsConfiguration> MessageLogsConfigurations { get; internal set; }
        public DbSet<MessageLogsMessage> MessageLogsMessages { get; internal set; }
        public DbSet<MessagePinningConfiguration> MessagePinningConfigurations { get; internal set; }
        public DbSet<MessagePinningWebhook> MessagePinningWebhooks { get; internal set; }
        public DbSet<NoticeConfiguration> NoticeConfigurations { get; internal set; }
        public DbSet<PremiumSlot> PremiumSlots { get; internal set; }
        public DbSet<ReputationConfiguration> ReputationConfigurations { get; internal set; }
        public DbSet<ReputationMember> ReputationMembers { get; internal set; }
        public DbSet<RoleLinkingConfiguration> RoleLinkingConfigurations { get; internal set; }
        public DbSet<RolePersistConfiguration> RolePersistConfigurations { get; internal set; }
        public DbSet<RolePersistMember> RolePersistMembers { get; internal set; }
        public DbSet<ShardDetail> ShardDetails { get; internal set; }
        public DbSet<Subscription> Subscriptions { get; internal set; }
        public DbSet<User> Users { get; internal set; }
        public DbSet<VoiceLinkConfiguration> VoiceLinkConfigurations { get; internal set; }
        public DbSet<VoiceLinkChannel> VoiceLinkChannels { get; internal set; }
        public DbSet<VoiceRoleConfiguration> VoiceRoleConfigurations { get; internal set; }
        public DbSet<VoteChannelConfiguration> VoteChannelConfigurations { get; internal set; }

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
        }
    }
}
