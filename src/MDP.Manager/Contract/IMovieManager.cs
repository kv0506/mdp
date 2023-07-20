using MDP.ServiceModel;

namespace MDP.Manager.Contract
{
    public interface IMovieManager
    {
        Task<IList<Movie>?> SearchMoviesAsync(string query);

        Task<Movie?> GetMovieByTitleAsync(string title);
    }
}