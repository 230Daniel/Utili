using System.Xml;
using AutoMapper;
using Discord.Rest;
using NewDatabase.Entities;
using UtiliBackend.Models;

namespace UtiliBackend.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RestTextChannel, TextChannelModel>();
            
            CreateMap<RestVoiceChannel, VoiceChannelModel>();
            
            CreateMap<RestRole, RoleModel>();

            CreateMap<CoreConfiguration, CoreConfigurationModel>();
            
            CreateMap<AutopurgeConfiguration, AutopurgeConfigurationModel>()
                .ForMember(
                    dest => dest.Timespan, 
                    opt => opt.MapFrom(s => XmlConvert.ToString(s.Timespan)));
            
            CreateMap<ChannelMirroringConfiguration, ChannelMirroringConfigurationModel>();

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

            CreateMap<JoinRolesConfiguration, JoinRolesConfigurationModel>();

            CreateMap<MessageFilterConfiguration, MessageFilterConfigurationModel>();

            CreateMap<MessageLogsConfiguration, MessageLogsConfigurationModel>();

            CreateMap<MessagePinningConfiguration, MessagePinningConfigurationModel>();
            
            CreateMap<NoticeConfiguration, NoticeConfigurationModel>()
                .ForMember(
                    dest => dest.Delay,
                    opt => opt.MapFrom(s => XmlConvert.ToString(s.Delay)))
                .ForMember(
                    dest => dest.Colour,
                    opt => opt.MapFrom(s => s.Colour.ToString("X6")));

            CreateMap<ReputationConfiguration, ReputationConfigurationModel>();
            CreateMap<ReputationConfigurationEmoji, ReputationConfigurationEmojiModel>();

            CreateMap<RoleLinkingConfiguration, RoleLinkingConfigurationModel>();

            CreateMap<RolePersistConfiguration, RolePersistConfigurationModel>();
            
            CreateMap<PremiumSlot, PremiumSlotModel>();
            
            CreateMap<Subscription, SubscriptionModel>()
                .ForMember(
                    dest => dest.ExpiresAt, 
                    opt => opt.MapFrom(s => XmlConvert.ToString(s.ExpiresAt, XmlDateTimeSerializationMode.Utc)));
        }
    }
}
