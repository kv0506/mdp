using AutoMapper;
using MDP.Caching.Contract;
using MDP.Manager.Contract;
using MDP.OMDb.Contract;
using MDP.OMDb.Model;
using MDP.ServiceModel;
using MDP.Videos.Contract;
using Moq;
using Moq.AutoMock;
using Shouldly;

namespace MDP.Manager.Tests
{
    [TestFixture]
    public class MovieManagerTests
    {
        private IMovieManager _movieManager;
        private Mock<IOMDbService> _omdbServiceMock;
        private Mock<IYoutubeService> _youtubeServiceMock;
        private Mock<ICacheClient> _cacheClientMock;

        [OneTimeSetUp]
        public void SetupFixture()
        {
            var mocker = new AutoMocker();

            var config = new MapperConfiguration(cfg => { cfg.AddProfile<OMDbProfile>(); });
            mocker.Use(config.CreateMapper());

            _movieManager = mocker.CreateInstance<MovieManager>();

            _omdbServiceMock = mocker.GetMock<IOMDbService>();
            _youtubeServiceMock = mocker.GetMock<IYoutubeService>();
            _cacheClientMock = mocker.GetMock<ICacheClient>();
        }

        [TearDown]
        public void TearDown()
        {
            _omdbServiceMock.Reset();
            _youtubeServiceMock.Reset();
            _cacheClientMock.Reset();
        }

        [Test]
        public async Task SearchMoviesAsync_ShouldReturnNull_When_NoMoviesFound()
        {
            _omdbServiceMock.Setup(x => x.SearchMoviesAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            var moviesResponse = await _movieManager.SearchMoviesAsync("wall");
            moviesResponse.ShouldBeNull();

            _omdbServiceMock.Verify(x => x.SearchMoviesAsync("wall"), Times.Once);
        }

        [Test]
        public async Task SearchMoviesAsync_ShouldFetchDataFromService_When_DataIsNotCached()
        {
            _omdbServiceMock.Setup(x => x.SearchMoviesAsync(It.IsAny<string>())).ReturnsAsync(() => new List<OMDbMovie>
            {
                new() { Title = "The Wall", Released = "2012" }
            });

            _cacheClientMock.Setup(x => x.GetAsync<IList<Movie>?>(It.IsAny<string>())).ReturnsAsync(() => null);

            var moviesResponse = await _movieManager.SearchMoviesAsync("wall");
            moviesResponse.ShouldNotBeNull();
            moviesResponse.Count.ShouldBe(1);
            moviesResponse.ShouldAllBe(x => x.Title.Contains("wall", StringComparison.InvariantCultureIgnoreCase));

            _omdbServiceMock.Verify(x => x.SearchMoviesAsync("wall"), Times.Once);
            _cacheClientMock.Verify(x => x.GetAsync<IList<Movie>?>("IEnumerable:Movie:wall"), Times.Once);
        }

        [Test]
        public async Task SearchMoviesAsync_ShouldNotFetchDataFromService_When_DataIsCached()
        {
            _cacheClientMock.Setup(x => x.GetAsync<IList<Movie>?>(It.IsAny<string>())).ReturnsAsync(() =>
                new List<Movie>
                {
                    new() { Title = "The Wall", Released = "2012" }
                });

            var moviesResponse = await _movieManager.SearchMoviesAsync("wall");
            moviesResponse.ShouldNotBeNull();
            moviesResponse.Count.ShouldBe(1);
            moviesResponse.ShouldAllBe(x => x.Title.Contains("wall", StringComparison.InvariantCultureIgnoreCase));

            _omdbServiceMock.Verify(x => x.SearchMoviesAsync("wall"), Times.Never);
            _cacheClientMock.Verify(x => x.GetAsync<IList<Movie>?>("IEnumerable:Movie:wall"), Times.Once);
        }

        [Test]
        public async Task GetByTitleAsync_ShouldReturnNull_When_NoMoviesFound()
        {
            _omdbServiceMock.Setup(x => x.GetMovieByTitleAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            var movieResponse = await _movieManager.GetMovieByTitleAsync("The Wolf");
            movieResponse.ShouldBeNull();

            _omdbServiceMock.Verify(x => x.GetMovieByTitleAsync("The Wolf"), Times.Once);
            _youtubeServiceMock.Verify(x => x.SearchVideosAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetByTitleAsync_ShouldFetchDataFromService_When_DataIsNotCached()
        {
            var movie = new OMDbMovie { Title = "The Wall", Year = "2012", Type = "Movie", Director = "xyz" };

            _omdbServiceMock.Setup(x => x.GetMovieByTitleAsync(It.IsAny<string>()))
                .ReturnsAsync(() => movie);

            _youtubeServiceMock.Setup(x => x.SearchVideosAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new List<Video>()
                {
                    new() { Title = "The Wall 2012 Full movie" }
                });

            _cacheClientMock.Setup(x => x.GetAsync<Movie?>(It.IsAny<string>())).ReturnsAsync(() => null);

            var movieResponse = await _movieManager.GetMovieByTitleAsync("The Wall");
            movieResponse.ShouldNotBeNull();
            movieResponse.Title.ShouldBe(movie.Title);
            movieResponse.Year.ShouldBe(movie.Year);
            movieResponse.Type.ShouldBe(movie.Type);

            movieResponse.Videos.ShouldNotBeNull();
            movieResponse.Videos.Count.ShouldBe(1);

            _omdbServiceMock.Verify(x => x.GetMovieByTitleAsync("The Wall"), Times.Once);
            _youtubeServiceMock.Verify(
                x => x.SearchVideosAsync($"{movie.Title} {movie.Year} {movie.Type} {movie.Director}"), Times.Once);
            _cacheClientMock.Verify(x => x.GetAsync<Movie?>("Movie:The Wall"), Times.Once);
        }

        [Test]
        public async Task GetByTitleAsync_ShouldNotFetchDataFromService_When_DataIsCached()
        {
            var movie = new Movie { Title = "The Wall", Year = "2012", Type = "Movie", Director = "xyz" };

            _cacheClientMock.Setup(x => x.GetAsync<Movie?>(It.IsAny<string>()))
                .ReturnsAsync(() => movie);

            var movieResponse = await _movieManager.GetMovieByTitleAsync("The Wall");
            movieResponse.ShouldNotBeNull();
            movieResponse.Title.ShouldBe(movie.Title);
            movieResponse.Year.ShouldBe(movie.Year);
            movieResponse.Type.ShouldBe(movie.Type);

            _omdbServiceMock.Verify(x => x.GetMovieByTitleAsync("The Wall"), Times.Never);
            _youtubeServiceMock.Verify(x => x.SearchVideosAsync(It.IsAny<string>()), Times.Never);
            _cacheClientMock.Verify(x => x.GetAsync<Movie?>("Movie:The Wall"), Times.Once);
        }
    }
}