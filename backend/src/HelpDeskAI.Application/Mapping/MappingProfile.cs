using AutoMapper;
using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Application.DTOs;
using HelpDeskAI.Domain.Entities;

namespace HelpDeskAI.Application.Mapping;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Comment, CommentDto>();
        CreateMap<Ticket, TicketDto>();
        CreateMap<DashboardData, DashboardDto>();
    }
}
