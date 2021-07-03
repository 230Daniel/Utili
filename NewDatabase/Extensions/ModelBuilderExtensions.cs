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
                .Where(x => x.IsAssignableTo(typeof(GuildEntity)));
            
            foreach (var type in types)
            {
                modelBuilder.Entity(type).HasKey("GuildId");
                modelBuilder.Entity(type).Property("GuildId").ValueGeneratedNever();
            }
        }

        public static void ConfigureGuildChannelEntities(this ModelBuilder modelBuilder)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsAssignableTo(typeof(GuildChannelEntity)));
            
            foreach (var type in types)
            {
                modelBuilder.Entity(type).HasKey("GuildId", "ChannelId");
                modelBuilder.Entity(type).Property("GuildId").ValueGeneratedNever();
                modelBuilder.Entity(type).Property("ChannelId").ValueGeneratedNever();
            }
        }
    }
}
