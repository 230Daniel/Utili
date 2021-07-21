using System.Xml;
using AutoMapper;
using Discord.Rest;
using NewDatabase.Entities;
using UtiliBackend.Models;
using UtiliBackend.Models.Dashboard;

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
        }
    }
}
