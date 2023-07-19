using AutoMapper;
using Google.Apis.YouTube.v3.Data;

namespace MDP.Videos;

public class YoutubeProfile : Profile
{
    public YoutubeProfile()
    {
        CreateMap<SearchResult, ServiceModel.Video>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id.VideoId))
            .ForMember(x => x.Title, opt => opt.MapFrom(src => src.Snippet.Title))
            .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Snippet.Description))
            .ForMember(x => x.Thumbnails, opt => opt.MapFrom(src => src.Snippet.Thumbnails));

        CreateMap<ThumbnailDetails, ServiceModel.ThumbnailDetails>();
        CreateMap<Thumbnail, ServiceModel.Thumbnail>();
    }
}