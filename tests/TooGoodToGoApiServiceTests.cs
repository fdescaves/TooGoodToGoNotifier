using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RestSharp;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Api.Responses;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier.Tests
{
    [TestFixture]
    public class TooGoodToGoApiServiceTests
    {
        private IOptions<ApiOptions> _apiOptions;
        private Mock<IRestClient> _restClientMock;

        [SetUp]
        public void SetUp()
        {
            _apiOptions = Options.Create(new ApiOptions { AuthenticationOptions = new AuthenticationOptions() });
            _restClientMock = new Mock<IRestClient>();
        }

        [Test]
        public void Authenticate_Should_Throw_When_ResponseIsNotSuccessful()
        {
            var restResponseMock = new Mock<IRestResponse<AuthenticationResponse>>();
            restResponseMock.Setup(x => x.IsSuccessful).Returns(false);
            _restClientMock.Setup(x => x.ExecuteAsync<AuthenticationResponse>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(restResponseMock.Object));

            var service = new TooGoodToGoApiService(_apiOptions, _restClientMock.Object);

            Assert.ThrowsAsync<TooGoodToGoRequestException>(async () => await service.Authenticate());
        }

        [Test]
        public async Task Authenticate_Should_DoPostRequestToConfiguredEndpoint()
        {
            _apiOptions.Value.BaseUrl = "baseUrl";
            _apiOptions.Value.AuthenticateEndpoint = "/login";

            var restResponseMock = new Mock<IRestResponse<AuthenticationResponse>>();
            restResponseMock.Setup(x => x.IsSuccessful).Returns(true);
            restResponseMock.Setup(x => x.Data).Returns(GetAuthenticationResponse());
            _restClientMock.Setup(x => x.ExecuteAsync<AuthenticationResponse>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(restResponseMock.Object));

            var service = new TooGoodToGoApiService(_apiOptions, _restClientMock.Object);
            await service.Authenticate();

            _restClientMock.Verify(x => x.ExecuteAsync<AuthenticationResponse>(It.Is<IRestRequest>(r => r.Resource == $"{_apiOptions.Value.BaseUrl}{_apiOptions.Value.AuthenticateEndpoint}" && r.Method == Method.POST), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Authenticate_Should_Returns_AuthenticationContext_With_ResponseData_When_ResponseIsSuccessful()
        {
            var authenticationResponse = GetAuthenticationResponse();
            var restResponseMock = new Mock<IRestResponse<AuthenticationResponse>>();
            restResponseMock.Setup(x => x.IsSuccessful).Returns(true);
            restResponseMock.Setup(x => x.Data).Returns(authenticationResponse);

            _restClientMock.Setup(x => x.ExecuteAsync<AuthenticationResponse>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(restResponseMock.Object));

            var service = new TooGoodToGoApiService(_apiOptions, _restClientMock.Object);
            var authenticationContext = await service.Authenticate();

            Assert.AreEqual(authenticationResponse.AccessToken, authenticationContext.AccessToken);
            Assert.AreEqual(authenticationResponse.RefreshToken, authenticationContext.RefreshToken);
            Assert.AreEqual(authenticationResponse.StartupData.User.UserId, authenticationContext.UserId);
        }

        [Test]
        public void GetFavoriteBaskets_Should_Throw_When_ResponseIsNotSuccessful()
        {
            var restResponseMock = new Mock<IRestResponse<GetBasketsResponse>>();
            restResponseMock.Setup(x => x.IsSuccessful).Returns(false);
            _restClientMock.Setup(x => x.ExecuteAsync<GetBasketsResponse>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(restResponseMock.Object));

            var service = new TooGoodToGoApiService(_apiOptions, _restClientMock.Object);

            Assert.ThrowsAsync<TooGoodToGoRequestException>(async () => await service.GetFavoriteBaskets("foo", 1));
        }

        [Test]
        public async Task GetFavoriteBaskets_Should_DoPostRequestToConfiguredEndpoint()
        {
            _apiOptions.Value.BaseUrl = "baseUrl";
            _apiOptions.Value.GetItemsEndpoint = "/items";

            var getFavoriteBasketsResponse = new GetBasketsResponse();
            var restResponseMock = new Mock<IRestResponse<GetBasketsResponse>>();
            restResponseMock.Setup(x => x.IsSuccessful).Returns(true);
            restResponseMock.Setup(x => x.Data).Returns(getFavoriteBasketsResponse);
            _restClientMock.Setup(x => x.ExecuteAsync<GetBasketsResponse>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(restResponseMock.Object));

            var service = new TooGoodToGoApiService(_apiOptions, _restClientMock.Object);
            await service.GetFavoriteBaskets("foo", 1);

            _restClientMock.Verify(x => x.ExecuteAsync<GetBasketsResponse>(It.Is<IRestRequest>(r => r.Resource == $"{_apiOptions.Value.BaseUrl}{_apiOptions.Value.GetItemsEndpoint}" && r.Method == Method.POST), It.IsAny<CancellationToken>()));
        }

        private static AuthenticationResponse GetAuthenticationResponse()
        {
            return new AuthenticationResponse
            {
                AccessToken = "foo",
                RefreshToken = "bar",
                StartupData = new StartupData
                {
                    User = new User
                    {
                        UserId = 1
                    }
                }
            };
        }
    }
}
