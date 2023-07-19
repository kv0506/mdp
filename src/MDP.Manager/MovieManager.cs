using AutoMapper;
using MDP.Caching.Contract;
using MDP.Manager.Contract;
using MDP.OMDb.Contract;
using MDP.ServiceModel;
using MDP.Videos.Contract;
using Microsoft.Extensions.Logging;

namespace MDP.Manager;

public class MovieManager : IMovieManager
{
    private readonly IOMDbService _omdbService;
    private readonly IYoutubeService _youtubeService;
    private readonly ICacheClient _cacheClient;
    private readonly IMapper _mapper;
    private readonly ILogger<MovieManager> _logger;

    public MovieManager(IOMDbService omdbService, IYoutubeService youtubeService, ICacheClient cacheClient,
        IMapper mapper, ILogger<MovieManager> logger)
    {
        _omdbService = omdbService;
        _youtubeService = youtubeService;
        _cacheClient = cacheClient;
        _mapper = mapper;
        _logger = logger;
    }

    public Task<Movie?> GetByIdAsync(string title)
    {
        return GetDataFromCacheOrDataSourceAsync<Movie?>(GetCacheKey<Movie>(title),
            () => GetByIdInternalAsync(title),
            TimeSpan.FromHours(2));
    }

    public Task<IList<Movie>?> SearchMoviesAsync(string query)
    {
        return GetDataFromCacheOrDataSourceAsync<IList<Movie>?>(GetCacheKey<IList<Movie>>(query),
            () => SearchMoviesInternalAsync(query),
            TimeSpan.FromHours(2));
    }

    private async Task<T> GetDataFromCacheOrDataSourceAsync<T>(string cacheKey, Func<Task<T>> fetchFromDataSource,
        TimeSpan? cacheExpiration)
    {
        var data = await _cacheClient.GetAsync<T>(cacheKey);
        if (data == null)
        {
            data = await fetchFromDataSource();
            if (data != null)
            {
                await _cacheClient.SetAsync(cacheKey, data, cacheExpiration);
            }
        }

        return data;
    }

    private async Task<Movie?> GetByIdInternalAsync(string id)
    {
        var movieResponse = await _omdbService.GetMovieByIdAsync(id);
        if (movieResponse != null)
        {
            var movie = _mapper.Map<Movie>(movieResponse);
            if (movie != null)
            {
                movie.Videos =
                    await SearchYoutubeVideosAsync($"{movie.Title} {movie.Year} {movie.Type} {movie.Director}");
                return movie;
            }
        }

        return null;
    }

    private async Task<IList<Movie>?> SearchMoviesInternalAsync(string query)
    {
        var omdbMoviesResponse = await _omdbService.SearchMoviesAsync(query);
        if (omdbMoviesResponse.Any())
        {
            return _mapper.Map<IList<Movie>>(omdbMoviesResponse);
        }

        return null;
    }

    private Task<IList<Video>?> SearchYoutubeVideosAsync(string query)
    {
        return _youtubeService.SearchVideosAsync(query);
    }

    private string GetCacheKey<T>(string key)
    {
        return $"{nameof(T)}:{key}";
    }
}