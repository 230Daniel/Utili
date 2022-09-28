using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Utili.Database.Entities;
using Utili.Database.Entities.Base;

namespace Utili.Database.Extensions;

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
        modelBuilder.Entity<CustomerDetails>().HasKey(e => e.CustomerId);
        modelBuilder.Entity<CustomerDetails>().Property(e => e.CustomerId).ValueGeneratedNever();

        modelBuilder.Entity<MessageLogsMessage>().HasIndex(e => e.Timestamp);

        modelBuilder.Entity<PremiumSlot>().HasKey(e => e.SlotId);
        modelBuilder.Entity<PremiumSlot>().Property(e => e.SlotId).ValueGeneratedOnAdd();

        modelBuilder.Entity<ReputationConfigurationEmoji>().HasKey("ReputationConfigurationGuildId", "Emoji");

        modelBuilder.Entity<RoleLinkingConfiguration>().HasKey(e => e.Id);
        modelBuilder.Entity<RoleLinkingConfiguration>().Property(e => e.Id).ValueGeneratedOnAdd();

        modelBuilder.Entity<Subscription>().HasKey(e => e.Id);
        modelBuilder.Entity<Subscription>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<User>().HasKey(e => e.UserId);
        modelBuilder.Entity<User>().Property(e => e.UserId).ValueGeneratedNever();
    }
}
