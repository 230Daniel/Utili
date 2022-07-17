using System.Xml;
using AutoMapper;
using Utili.Database.Entities;
using Disqord;
using Utili.Backend.Models;

namespace Utili.Backend.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        MapDiscordModels();
        MapDashboardModels();
        MapPremiumModels();
    }

    private void MapDiscordModels()
    {
        CreateMap<ITextChannel, TextChannelModel>();
        CreateMap<IVocalGuildChannel, VocalChannelModel>();
        CreateMap<IRole, RoleModel>();
    }

    private void MapDashboardModels()
    {
        CreateMap<CoreConfiguration, CoreConfigurationModel>();
        CreateMap<ChannelMirroringConfiguration, ChannelMirroringConfigurationModel>();
        CreateMap<JoinRolesConfiguration, JoinRolesConfigurationModel>();
        CreateMap<MessageFilterConfiguration, MessageFilterConfigurationModel>();
        CreateMap<MessageLogsConfiguration, MessageLogsConfigurationModel>();
        CreateMap<MessagePinningConfiguration, MessagePinningConfigurationModel>();
        CreateMap<ReputationConfiguration, ReputationConfigurationModel>();
        CreateMap<ReputationConfigurationEmoji, ReputationConfigurationEmojiModel>();
        CreateMap<RoleLinkingConfiguration, RoleLinkingConfigurationModel>();
        CreateMap<RolePersistConfiguration, RolePersistConfigurationModel>();
        CreateMap<VoiceLinkConfiguration, VoiceLinkConfigurationModel>();
        CreateMap<VoiceRoleConfiguration, VoiceRoleConfigurationModel>();
        CreateMap<VoteChannelConfiguration, VoteChannelConfigurationModel>();

        CreateMap<AutopurgeConfiguration, AutopurgeConfigurationModel>()
            .ForMember(
                dest => dest.Timespan,
                opt => opt.MapFrom(s => XmlConvert.ToString(s.Timespan)));

        CreateMap<InactiveRoleConfiguration, InactiveRoleConfigurationModel>()
            .ForMember(
                dest => dest.Threshold,
                opt => opt.MapFrom(s => XmlConvert.ToString(s.Threshold)))
            .ForMember(
                dest => dest.AutoKickThreshold,
                opt => opt.MapFrom(s => XmlConvert.ToString(s.AutoKickThreshold)));

        CreateMap<JoinMessageConfiguration, JoinMessageConfigurationModel>()
            .ForMember(
                dest => dest.Colour,
                opt => opt.MapFrom(s => s.Colour.ToString("X6")));

        CreateMap<NoticeConfiguration, NoticeConfigurationModel>()
            .ForMember(
                dest => dest.Delay,
                opt => opt.MapFrom(s => XmlConvert.ToString(s.Delay)))
            .ForMember(
                dest => dest.Colour,
                opt => opt.MapFrom(s => s.Colour.ToString("X6")));
    }

    private void MapPremiumModels()
    {
        CreateMap<PremiumSlot, PremiumSlotModel>();

        CreateMap<Subscription, SubscriptionModel>()
            .ForMember(
                dest => dest.ExpiresAt,
                opt => opt.MapFrom(s => XmlConvert.ToString(s.ExpiresAt, XmlDateTimeSerializationMode.Utc)));
    }
}