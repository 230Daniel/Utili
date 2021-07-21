using AutoMapper;
using Discord.Rest;
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
        }
    }
}
