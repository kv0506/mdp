using System.Text.Json;
using MDP.Exceptions;
using MDP.OMDb.Contract;
using MDP.OMDb.Model;
using Microsoft.Extensions.Options;

namespace MDP.OMDb;

public class OMDbService : IOMDbService
{
    private readonly OMDbSettings _omdbSettings;
    private readonly IHttpClientFactory _httpClientFactory;

    public OMDbService(IOptions<OMDbSettings> omdbSettings, IHttpClientFactory httpClientFactory)
    {
        _omdbSettings = omdbSettings.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IList<OMDbMovie>?> SearchMoviesAsync(string query)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var response = await httpClient.GetAsync($"{GetOMDbBaseUrl()}s={query}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var searchResult = JsonSerializer.Deserialize<SearchResponse>(responseContent);
            if (searchResult != null)
            {
                if (searchResult.Response.IsTrue())
                {
                    return searchResult.Search;
                }

                throw new MDPException(ErrorCode.BadRequest, searchResult.Error);
            }
        }

        throw new MDPException(ErrorCode.BadRequest, responseContent);
    }

    public async Task<OMDbMovie?> GetMovieByIdAsync(string id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var response = await httpClient.GetAsync($"{GetOMDbBaseUrl()}i={id}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var movie = JsonSerializer.Deserialize<OMDbMovie>(responseContent);
            if (movie != null)
            {
                if (movie.Response.IsTrue())
                {
                    return movie;
                }

                throw new MDPException(ErrorCode.NotFound, movie.Error);
            }
        }

        throw new MDPException(ErrorCode.BadRequest, responseContent);
    }

    private string GetOMDbBaseUrl()
    {
        return $"{_omdbSettings.ApiUrl}?apikey={_omdbSettings.ApiKey}&type=movie&r=json&";
    }
}