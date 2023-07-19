using MDP.API.Model;
using MDP.Manager.Contract;
using MDP.ServiceModel;
using Microsoft.AspNetCore.Mvc;

namespace MDP.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieManager _movieManager;
        private readonly ILogger<MovieController> _logger;

        public MovieController(IMovieManager movieManager, ILogger<MovieController> logger)
        {
            _logger = logger;
            _movieManager = movieManager;
        }

        [HttpGet, Route("search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IList<Movie>>))]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _movieManager.SearchMoviesAsync(query);
            return Ok(new ApiResponse<IList<Movie>> { IsSuccess = true, Result = result });
        }

        [HttpGet, Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<Movie>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetByTitle([FromRoute] string id)
        {
            var result = await _movieManager.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ApiResponse<Movie>
                    { IsSuccess = false, Errors = new[] { new Error("NotFound", "Movie not found") } });
            }

            return Ok(new ApiResponse<Movie> { IsSuccess = true, Result = result });
        }
    }
}