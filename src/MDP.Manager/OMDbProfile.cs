using AutoMapper;
using MDP.OMDb.Model;
using MDP.ServiceModel;

namespace MDP.Manager;

public class OMDbProfile : Profile
{
    public OMDbProfile()
    {
        CreateMap<OMDbMovie, Movie>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.IMDbID));

        CreateMap<OMDbRating, Rating>();
    }
}