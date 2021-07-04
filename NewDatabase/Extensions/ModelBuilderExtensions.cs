using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NewDatabase.Entities;
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
        
        public static void ConfigureMemberEntities(this ModelBuilder modelBuilder)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsAssignableTo(typeof(MemberEntity)) && !x.IsEquivalentTo(typeof(MemberEntity)));
            
            foreach (var type in types)
            {
                modelBuilder.Entity(type).HasKey("GuildId", "MemberId");
                modelBuilder.Entity(type).Property("GuildId").ValueGeneratedNever();
                modelBuilder.Entity(type).Property("MemberId").ValueGeneratedNever();
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
            modelBuilder.Entity<MessageLogsMessage>().HasIndex(e => e.Timestamp);
        }
        
        public static void ConfigureUlongListConverters(this ModelBuilder modelBuilder)
        {
            var ulongListConverter = new ValueConverter<List<ulong>, decimal[]>(
                ulongs => ulongs.Select(Convert.ToDecimal).ToArray(),
                decimals => decimals.Select(Convert.ToUInt64).ToList());

            foreach (var type in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in type.GetProperties())
                {
                    if (property.ClrType == typeof(List<ulong>))
                    {
                        property.SetValueConverter(ulongListConverter);
                        property.SetColumnType("numeric(20,0)[]");
                    }
                }
            }
        }
    }
}
