using System.Text.Json;
using CSharpExtensions;
using MDP.OMDb.Contract;
using MDP.OMDb.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MDP.OMDb;

public class OMDbService : IOMDbService
{
    private readonly OMDbSettings _omdbSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OMDbService> _logger;

    public OMDbService(IOptions<OMDbSettings> omdbSettings, IHttpClientFactory httpClientFactory, ILogger<OMDbService> logger)
    {
        _omdbSettings = omdbSettings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IList<OMDbMovie>> SearchMoviesAsync(string query)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync($"{GetOMDbBaseUrl()}s={query}");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var searchResult = JsonSerializer.Deserialize<SearchResponse>(responseContent);
            return searchResult != null ? searchResult.Search : new List<OMDbMovie>();
        }

        _logger.LogDebug($"No movies found for the query {query}");
        return new List<OMDbMovie>();
    }

    public async Task<OMDbMovie?> GetMovieByIdAsync(string id)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync($"{GetOMDbBaseUrl()}i={id}");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var movie = JsonSerializer.Deserialize<OMDbMovie>(responseContent);
            if (movie != null && movie.Error.IsNullOrEmpty() &&
                movie.Response.IsEquals(true.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return movie;
            }
        }

        _logger.LogDebug($"No movie found by the id {id}");
        return null;
    }

    private string GetOMDbBaseUrl()
    {
        return $"{_omdbSettings.ApiUrl}?apikey={_omdbSettings.ApiKey}&type=movie&r=json&";
    }
}