using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NewDatabase.Entities.Base;

namespace NewDatabase.Extensions
{
    internal static class ModelBuilderExtensions
    {
        public static void ConfigureGuildEntities(this ModelBuilder modelBuilder)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsAssignableTo(typeof(GuildEntity)) && !x.IsEquivalentTo(typeof(GuildEntity)));
            
            foreach (var type in types)
            {
                modelBuilder.Entity(type).HasKey("GuildId");
                modelBuilder.Entity(type).Property("GuildId").ValueGeneratedNever();
            }
        }

        public static void ConfigureGuildChannelEntities(this ModelBuilder modelBuilder)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsAssignableTo(typeof(GuildChannelEntity)) && !x.IsEquivalentTo(typeof(GuildChannelEntity)));
            
            foreach (var type in types)
            {
                modelBuilder.Entity(type).HasKey("GuildId", "ChannelId");
                modelBuilder.Entity(type).Property("GuildId").ValueGeneratedNever();
                modelBuilder.Entity(type).Property("ChannelId").ValueGeneratedNever();
            }
        }
        
        public static void ConfigureMessageEntities(this ModelBuilder modelBuilder)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsAssignableTo(typeof(MessageEntity)) && !x.IsEquivalentTo(typeof(MessageEntity)));
            
            foreach (var type in types)
            {
                modelBuilder.Entity(type).HasKey("MessageId");
                modelBuilder.Entity(type).Property("MessageId").ValueGeneratedNever();
            }
        }

        public static void ConfigureOtherEntities(this ModelBuilder modelBuilder)
        {
            
        }
    }
}
