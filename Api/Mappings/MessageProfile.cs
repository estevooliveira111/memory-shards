using Api.DTOs;
using Api.Entities;
using AutoMapper;

namespace Api.Mappings;

public sealed class MessageProfile : Profile
{
    public MessageProfile()
    {
        // Entity → Response DTOs
        CreateMap<TemporaryMessage, CreateMessageResponse>()
            .ForMember(dest => dest.Url, opt => opt.Ignore()); // URL is built by the service

        CreateMap<TemporaryMessage, GetMessageResponse>()
            .ForMember(dest => dest.Content, opt => opt.Ignore()); // Content requires decryption
    }
}
