﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NewDatabase;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NewDatabase.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("NewDatabase.Entities.AutopurgeConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<bool>("AddedFromDashboard")
                        .HasColumnType("boolean")
                        .HasColumnName("added_from_dashboard");

                    b.Property<int>("Mode")
                        .HasColumnType("integer")
                        .HasColumnName("mode");

                    b.Property<TimeSpan>("Timespan")
                        .HasColumnType("interval")
                        .HasColumnName("timespan");

                    b.HasKey("GuildId", "ChannelId")
                        .HasName("pk_autopurge_configurations");

                    b.ToTable("autopurge_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.AutopurgeMessage", b =>
                {
                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<bool>("IsBot")
                        .HasColumnType("boolean")
                        .HasColumnName("is_bot");

                    b.Property<bool>("IsPinned")
                        .HasColumnType("boolean")
                        .HasColumnName("is_pinned");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("timestamp");

                    b.HasKey("MessageId")
                        .HasName("pk_autopurge_messages");

                    b.ToTable("autopurge_messages");
                });

            modelBuilder.Entity("NewDatabase.Entities.ChannelMirroringConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<decimal>("DestinationChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("destination_channel_id");

                    b.Property<decimal>("WebhookId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("webhook_id");

                    b.HasKey("GuildId", "ChannelId")
                        .HasName("pk_channel_mirroring_configurations");

                    b.ToTable("channel_mirroring_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.CoreConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<int>("BotFeatures")
                        .HasColumnType("integer")
                        .HasColumnName("bot_features");

                    b.Property<bool>("CommandsEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("commands_enabled");

                    b.Property<decimal[]>("NonCommandChannels")
                        .HasColumnType("numeric(20,0)[]")
                        .HasColumnName("non_command_channels");

                    b.Property<string>("Prefix")
                        .HasColumnType("text")
                        .HasColumnName("prefix");

                    b.HasKey("GuildId")
                        .HasName("pk_core_configurations");

                    b.ToTable("core_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.CustomerDetails", b =>
                {
                    b.Property<string>("CustomerId")
                        .HasColumnType("text")
                        .HasColumnName("customer_id");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("CustomerId")
                        .HasName("pk_customer_details");

                    b.ToTable("customer_details");
                });

            modelBuilder.Entity("NewDatabase.Entities.InactiveRoleConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<bool>("AutoKick")
                        .HasColumnType("boolean")
                        .HasColumnName("auto_kick");

                    b.Property<TimeSpan>("AutoKickThreshold")
                        .HasColumnType("interval")
                        .HasColumnName("auto_kick_threshold");

                    b.Property<DateTime>("DefaultLastAction")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("default_last_action");

                    b.Property<decimal>("ImmuneRoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("immune_role_id");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_update");

                    b.Property<int>("Mode")
                        .HasColumnType("integer")
                        .HasColumnName("mode");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("role_id");

                    b.Property<TimeSpan>("Threshold")
                        .HasColumnType("interval")
                        .HasColumnName("threshold");

                    b.HasKey("GuildId")
                        .HasName("pk_inactive_role_configurations");

                    b.ToTable("inactive_role_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.InactiveRoleMember", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("MemberId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("member_id");

                    b.Property<DateTime>("LastAction")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_action");

                    b.HasKey("GuildId", "MemberId")
                        .HasName("pk_inactive_role_members");

                    b.ToTable("inactive_role_members");
                });

            modelBuilder.Entity("NewDatabase.Entities.JoinMessageConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<long>("Colour")
                        .HasColumnType("bigint")
                        .HasColumnName("colour");

                    b.Property<string>("Content")
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<bool>("Enabled")
                        .HasColumnType("boolean")
                        .HasColumnName("enabled");

                    b.Property<string>("Footer")
                        .HasColumnType("text")
                        .HasColumnName("footer");

                    b.Property<string>("Icon")
                        .HasColumnType("text")
                        .HasColumnName("icon");

                    b.Property<string>("Image")
                        .HasColumnType("text")
                        .HasColumnName("image");

                    b.Property<int>("Mode")
                        .HasColumnType("integer")
                        .HasColumnName("mode");

                    b.Property<string>("Text")
                        .HasColumnType("text")
                        .HasColumnName("text");

                    b.Property<string>("Thumbnail")
                        .HasColumnType("text")
                        .HasColumnName("thumbnail");

                    b.Property<string>("Title")
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.HasKey("GuildId")
                        .HasName("pk_join_message_configurations");

                    b.ToTable("join_message_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.JoinRolesConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal[]>("JoinRoles")
                        .HasColumnType("numeric(20,0)[]")
                        .HasColumnName("join_roles");

                    b.Property<bool>("WaitForVerification")
                        .HasColumnType("boolean")
                        .HasColumnName("wait_for_verification");

                    b.HasKey("GuildId")
                        .HasName("pk_join_roles_configurations");

                    b.ToTable("join_roles_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.JoinRolesPendingMember", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("MemberId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("member_id");

                    b.Property<bool>("IsPending")
                        .HasColumnType("boolean")
                        .HasColumnName("is_pending");

                    b.Property<DateTime>("ScheduledFor")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("scheduled_for");

                    b.HasKey("GuildId", "MemberId")
                        .HasName("pk_join_roles_pending_members");

                    b.ToTable("join_roles_pending_members");
                });

            modelBuilder.Entity("NewDatabase.Entities.MessageFilterConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<string>("DeletionMessage")
                        .HasColumnType("text")
                        .HasColumnName("deletion_message");

                    b.Property<int>("Mode")
                        .HasColumnType("integer")
                        .HasColumnName("mode");

                    b.Property<string>("RegEx")
                        .HasColumnType("text")
                        .HasColumnName("reg_ex");

                    b.HasKey("GuildId", "ChannelId")
                        .HasName("pk_message_filter_configurations");

                    b.ToTable("message_filter_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.MessageLogsConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("DeletedChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("deleted_channel_id");

                    b.Property<decimal>("EditedChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("edited_channel_id");

                    b.Property<decimal[]>("ExcludedChannels")
                        .HasColumnType("numeric(20,0)[]")
                        .HasColumnName("excluded_channels");

                    b.HasKey("GuildId")
                        .HasName("pk_message_logs_configurations");

                    b.ToTable("message_logs_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.MessageLogsMessage", b =>
                {
                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.Property<decimal>("AuthorId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("author_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<string>("Content")
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("timestamp");

                    b.HasKey("MessageId")
                        .HasName("pk_message_logs_messages");

                    b.HasIndex("Timestamp")
                        .HasDatabaseName("ix_message_logs_messages_timestamp");

                    b.ToTable("message_logs_messages");
                });

            modelBuilder.Entity("NewDatabase.Entities.MessagePinningConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("PinChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("pin_channel_id");

                    b.Property<bool>("PinMessages")
                        .HasColumnType("boolean")
                        .HasColumnName("pin_messages");

                    b.HasKey("GuildId")
                        .HasName("pk_message_pinning_configurations");

                    b.ToTable("message_pinning_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.MessagePinningWebhook", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<decimal>("WebhookId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("webhook_id");

                    b.HasKey("GuildId", "ChannelId")
                        .HasName("pk_message_pinning_webhooks");

                    b.ToTable("message_pinning_webhooks");
                });

            modelBuilder.Entity("NewDatabase.Entities.NoticeConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<long>("Colour")
                        .HasColumnType("bigint")
                        .HasColumnName("colour");

                    b.Property<string>("Content")
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<TimeSpan>("Delay")
                        .HasColumnType("interval")
                        .HasColumnName("delay");

                    b.Property<bool>("Enabled")
                        .HasColumnType("boolean")
                        .HasColumnName("enabled");

                    b.Property<string>("Footer")
                        .HasColumnType("text")
                        .HasColumnName("footer");

                    b.Property<string>("Icon")
                        .HasColumnType("text")
                        .HasColumnName("icon");

                    b.Property<string>("Image")
                        .HasColumnType("text")
                        .HasColumnName("image");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.Property<string>("Text")
                        .HasColumnType("text")
                        .HasColumnName("text");

                    b.Property<string>("Thumbnail")
                        .HasColumnType("text")
                        .HasColumnName("thumbnail");

                    b.Property<string>("Title")
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.Property<bool>("UpdatedFromDashboard")
                        .HasColumnType("boolean")
                        .HasColumnName("updated_from_dashboard");

                    b.HasKey("GuildId", "ChannelId")
                        .HasName("pk_notice_configurations");

                    b.ToTable("notice_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.PremiumSlot", b =>
                {
                    b.Property<int>("SlotId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("slot_id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("SlotId")
                        .HasName("pk_premium_slots");

                    b.ToTable("premium_slots");
                });

            modelBuilder.Entity("NewDatabase.Entities.ReputationConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.HasKey("GuildId")
                        .HasName("pk_reputation_configurations");

                    b.ToTable("reputation_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.ReputationConfigurationEmoji", b =>
                {
                    b.Property<decimal>("ReputationConfigurationGuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("reputation_configuration_guild_id");

                    b.Property<string>("Emoji")
                        .HasColumnType("text")
                        .HasColumnName("emoji");

                    b.Property<int>("Value")
                        .HasColumnType("integer")
                        .HasColumnName("value");

                    b.HasKey("ReputationConfigurationGuildId", "Emoji")
                        .HasName("pk_reputation_configuration_emoji");

                    b.ToTable("reputation_configuration_emoji");
                });

            modelBuilder.Entity("NewDatabase.Entities.ReputationMember", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("MemberId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("member_id");

                    b.Property<long>("Reputation")
                        .HasColumnType("bigint")
                        .HasColumnName("reputation");

                    b.HasKey("GuildId", "MemberId")
                        .HasName("pk_reputation_members");

                    b.ToTable("reputation_members");
                });

            modelBuilder.Entity("NewDatabase.Entities.RoleLinkingConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("LinkedRoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("linked_role_id");

                    b.Property<int>("Mode")
                        .HasColumnType("integer")
                        .HasColumnName("mode");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("role_id");

                    b.HasKey("Id")
                        .HasName("pk_role_linking_configurations");

                    b.ToTable("role_linking_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.RolePersistConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<bool>("Enabled")
                        .HasColumnType("boolean")
                        .HasColumnName("enabled");

                    b.Property<decimal[]>("ExcludedRoles")
                        .HasColumnType("numeric(20,0)[]")
                        .HasColumnName("excluded_roles");

                    b.HasKey("GuildId")
                        .HasName("pk_role_persist_configurations");

                    b.ToTable("role_persist_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.RolePersistMember", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("MemberId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("member_id");

                    b.Property<decimal[]>("Roles")
                        .HasColumnType("numeric(20,0)[]")
                        .HasColumnName("roles");

                    b.HasKey("GuildId", "MemberId")
                        .HasName("pk_role_persist_members");

                    b.ToTable("role_persist_members");
                });

            modelBuilder.Entity("NewDatabase.Entities.ShardDetail", b =>
                {
                    b.Property<int>("ShardId")
                        .HasColumnType("integer")
                        .HasColumnName("shard_id");

                    b.Property<int>("Guilds")
                        .HasColumnType("integer")
                        .HasColumnName("guilds");

                    b.Property<DateTime>("Heartbeat")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("heartbeat");

                    b.HasKey("ShardId")
                        .HasName("pk_shard_details");

                    b.ToTable("shard_details");
                });

            modelBuilder.Entity("NewDatabase.Entities.Subscription", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("expires_at");

                    b.Property<int>("Slots")
                        .HasColumnType("integer")
                        .HasColumnName("slots");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_subscriptions");

                    b.ToTable("subscriptions");
                });

            modelBuilder.Entity("NewDatabase.Entities.User", b =>
                {
                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<string>("Email")
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.HasKey("UserId")
                        .HasName("pk_users");

                    b.ToTable("users");
                });

            modelBuilder.Entity("NewDatabase.Entities.VoiceLinkChannel", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<decimal>("TextChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("text_channel_id");

                    b.HasKey("GuildId", "ChannelId")
                        .HasName("pk_voice_link_channels");

                    b.ToTable("voice_link_channels");
                });

            modelBuilder.Entity("NewDatabase.Entities.VoiceLinkConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("ChannelPrefix")
                        .HasColumnType("text")
                        .HasColumnName("channel_prefix");

                    b.Property<bool>("DeleteChannels")
                        .HasColumnType("boolean")
                        .HasColumnName("delete_channels");

                    b.Property<bool>("Enabled")
                        .HasColumnType("boolean")
                        .HasColumnName("enabled");

                    b.Property<decimal[]>("ExcludedChannels")
                        .HasColumnType("numeric(20,0)[]")
                        .HasColumnName("excluded_channels");

                    b.HasKey("GuildId")
                        .HasName("pk_voice_link_configurations");

                    b.ToTable("voice_link_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.VoiceRoleConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("role_id");

                    b.HasKey("GuildId", "ChannelId")
                        .HasName("pk_voice_role_configurations");

                    b.ToTable("voice_role_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.VoteChannelConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<List<string>>("Emojis")
                        .HasColumnType("text[]")
                        .HasColumnName("emojis");

                    b.Property<int>("Mode")
                        .HasColumnType("integer")
                        .HasColumnName("mode");

                    b.HasKey("GuildId", "ChannelId")
                        .HasName("pk_vote_channel_configurations");

                    b.ToTable("vote_channel_configurations");
                });

            modelBuilder.Entity("NewDatabase.Entities.ReputationConfigurationEmoji", b =>
                {
                    b.HasOne("NewDatabase.Entities.ReputationConfiguration", null)
                        .WithMany("Emojis")
                        .HasForeignKey("ReputationConfigurationGuildId")
                        .HasConstraintName("fk_reputation_configuration_emoji_reputation_configurations_re")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NewDatabase.Entities.ReputationConfiguration", b =>
                {
                    b.Navigation("Emojis");
                });
#pragma warning restore 612, 618
        }
    }
}
