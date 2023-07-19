using MDP.ServiceModel;

namespace MDP.Videos.Contract;

public interface IYoutubeService
{
    Task<IList<Video>?> SearchVideosAsync(string query);
}