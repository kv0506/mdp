using MDP.OMDb.Model;

namespace MDP.OMDb.Contract;

public interface IOMDbService
{
    Task<IList<OMDbMovie>?> SearchMoviesAsync(string query);

    Task<OMDbMovie?> GetMovieByTitleAsync(string title);
}