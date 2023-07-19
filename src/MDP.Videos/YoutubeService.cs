using AutoMapper;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MDP.Exceptions;
using MDP.ServiceModel;
using MDP.Videos.Contract;
using Microsoft.Extensions.Options;

namespace MDP.Videos
{
    public class YoutubeService : IYoutubeService
    {
        private readonly IMapper _mapper;
        private readonly YoutubeSettings _settings;

        public YoutubeService(IMapper mapper, IOptions<YoutubeSettings> settings)
        {
            _mapper = mapper;
            _settings = settings.Value;
        }

        public async Task<IList<Video>?> SearchVideosAsync(string query)
        {
            try
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = _settings.ApiKey,
                    ApplicationName = _settings.AppName
                });

                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = query;
                searchListRequest.MaxResults = 10;
                searchListRequest.Type = "video";
                searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;

                var searchListResponse = await searchListRequest.ExecuteAsync();
                return _mapper.Map<IList<Video>>(searchListResponse.Items);
            }
            catch (Exception ex)
            {
                throw new MDPException(ErrorCode.BadRequest, ex);
            }
        }
    }
}