using System.Net;
using MDP.Exceptions;
using MDP.OMDb.Contract;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Moq.Contrib.HttpClient;
using Shouldly;

namespace MDP.OMDb.Tests
{
    [TestFixture]
    public class OMDbServiceTests
    {
        private Mock<HttpMessageHandler> _messageHandlerMock;
        private IOMDbService _omdbService;

        [OneTimeSetUp]
        public void SetupFixture()
        {
            var mocker = new AutoMocker();

            mocker.Use(Options.Create(new OMDbSettings
            {
                ApiKey = "12548",
                ApiUrl = "http://www.omdbapi.com/"
            }));

            _messageHandlerMock = new Mock<HttpMessageHandler>();
            mocker.Use(_messageHandlerMock.CreateClientFactory());

            _omdbService = mocker.CreateInstance<OMDbService>();
        }

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
            _messageHandlerMock.Reset();
        }

        [Test]
        public async Task SearchMoviesAsync_ShouldThrow_BadRequestException_When_ResponseStatusCodeIsNotSuccessCode()
        {
            _messageHandlerMock.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.BadRequest, "Service returned error code");

            var exception = await Should.ThrowAsync<MDPException>(() => _omdbService.SearchMoviesAsync("test"));
            exception.ErrorCode.ShouldBe(ErrorCode.BadRequest);

            _messageHandlerMock.VerifyAnyRequest(Times.Once());
        }

        [Test]
        public async Task SearchMoviesAsync_ShouldThrow_BadRequestException_When_ResponseIsFalse()
        {
            _messageHandlerMock.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.OK, "{\"Response\":\"False\",\"Error\":\"Too many results.\"}");

            var exception = await Should.ThrowAsync<MDPException>(() => _omdbService.SearchMoviesAsync("k"));
            exception.ErrorCode.ShouldBe(ErrorCode.BadRequest);
            exception.Message.ShouldBe("Too many results.");

            _messageHandlerMock.VerifyAnyRequest(Times.Once());
        }

        [Test]
        public async Task SearchMoviesAsync_ShouldReturn_MoviesList_When_ResponseIsTrue()
        {
            _messageHandlerMock.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.OK, "{ \"Search\": [ { \"Title\": \"The Wolf of Wall Street\", \"Year\": \"2013\", \"imdbID\": \"tt0993846\", \"Type\": \"movie\" }, { \"Title\": \"Wall Street\", \"Year\": \"1987\", \"imdbID\": \"tt0094291\", \"Type\": \"movie\" }, { \"Title\": \"The Great Wall\", \"Year\": \"2016\", \"imdbID\": \"tt2034800\", \"Type\": \"movie\" }, { \"Title\": \"Pink Floyd: The Wall\", \"Year\": \"1982\", \"imdbID\": \"tt0084503\", \"Type\": \"movie\" }, { \"Title\": \"The Wall\", \"Year\": \"2017\", \"imdbID\": \"tt4218696\", \"Type\": \"movie\" } ], \"Response\": \"True\" }");

            var movies = await _omdbService.SearchMoviesAsync("wall");
            movies.ShouldNotBeNull();
            movies.Count.ShouldBe(5);
            movies.ShouldAllBe(x => x.Title.Contains("Wall") && x.Type.Equals("movie"));

            _messageHandlerMock.VerifyAnyRequest(Times.Once());
        }

        [Test]
        public async Task GetMovieByTitleAsync_ShouldThrow_BadRequestException_When_ResponseStatusCodeIsNotSuccessCode()
        {
            _messageHandlerMock.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.BadRequest, "Service returned error code");

            var exception = await Should.ThrowAsync<MDPException>(() => _omdbService.GetMovieByTitleAsync("test"));
            exception.ErrorCode.ShouldBe(ErrorCode.BadRequest);

            _messageHandlerMock.VerifyAnyRequest(Times.Once());
        }

        [Test]
        public async Task GetMovieByTitleAsync_ShouldThrow_BadRequestException_When_ResponseIsFalse()
        {
            _messageHandlerMock.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.OK, "{\"Response\":\"False\",\"Error\":\"No movie found.\"}");

            var exception = await Should.ThrowAsync<MDPException>(() => _omdbService.GetMovieByTitleAsync("k"));
            exception.ErrorCode.ShouldBe(ErrorCode.NotFound);
            exception.Message.ShouldBe("No movie found.");

            _messageHandlerMock.VerifyAnyRequest(Times.Once());
        }

        [Test]
        public async Task GetMovieByTitleAsync_ShouldReturn_Movie_When_ResponseIsTrue()
        {
            _messageHandlerMock.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.OK, "{\"Title\":\"Kamen Rider ZX: Birth of the 10th! Kamen Riders All Together!!\",\"Year\":\"1984\",\"Rated\":\"N/A\",\"Released\":\"03 Jan 1984\",\"Runtime\":\"45 min\",\"Genre\":\"Action, Adventure, Drama\",\"Director\":\"Minoru Yamada\",\"Writer\":\"Shotaro Ishinomori\",\"Actors\":\"Shun Sugata, Shunsuke Takasugi, Akira Yamaguchi\",\"Plot\":\"Ryo Murasame is an aircraft pilot. One day he is taking his sister out on a ride along the Amazon when they are shot down by a UFO. They survive only to be captured by the Badan Empire. His sister is killed, while Ryo becomes the ...\",\"Language\":\"Japanese\",\"Country\":\"Japan\",\"Awards\":\"N/A\",\"Ratings\":[{\"Source\":\"Internet Movie Database\",\"Value\":\"5.5/10\"}],\"Metascore\":\"N/A\",\"imdbRating\":\"5.5\",\"imdbVotes\":\"82\",\"imdbID\":\"tt0157881\",\"Type\":\"movie\",\"DVD\":\"N/A\",\"BoxOffice\":\"N/A\",\"Production\":\"N/A\",\"Website\":\"N/A\",\"Response\":\"True\"}");

            var movie = await _omdbService.GetMovieByTitleAsync("Kamen Rider");
            movie.ShouldNotBeNull();
            movie.Title.ShouldContain("Kamen Rider");

            _messageHandlerMock.VerifyAnyRequest(Times.Once());
        }
    }
}