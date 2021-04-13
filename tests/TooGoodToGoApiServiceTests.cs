using System.Security.Authentication;
using Microsoft.Extensions.Logging;
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
        private Mock<ILogger<TooGoodToGoApiService>> _loggerMock;
        private IOptions<ApiOptions> _apiOptions;
        private IOptions<AuthenticationOptions> _authenticationOptions;
        private Mock<IRestClient> _restClientMock;
        private AuthenticationResponse _authenticationResponse;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<TooGoodToGoApiService>>();
            _apiOptions = Options.Create(new ApiOptions());
            _authenticationOptions = Options.Create(new AuthenticationOptions());
            _restClientMock = new Mock<IRestClient>();
            _authenticationResponse = new AuthenticationResponse
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

        [Test]
        public void Authenticate_Should_Throw_When_ResponseIsNotSuccessful()
        {
            var restResponseMock = new Mock<IRestResponse<AuthenticationResponse>>();
            restResponseMock.Setup(x => x.IsSuccessful).Returns(false);
            _restClientMock.Setup(x => x.Execute<AuthenticationResponse>(It.IsAny<IRestRequest>())).Returns(restResponseMock.Object);

            var service = new TooGoodToGoApiService(_loggerMock.Object, _apiOptions, _authenticationOptions, _restClientMock.Object);

            Assert.Throws<AuthenticationException>(() => service.Authenticate());
        }

        [Test]
        public void Authenticate_Should_DoPostRequestToAuthenticateEndpoint()
        {
            _apiOptions.Value.BaseUrl = "foo";
            _apiOptions.Value.AuthenticateEndpoint = "bar";

            var restResponseMock = new Mock<IRestResponse<AuthenticationResponse>>();
            restResponseMock.Setup(x => x.IsSuccessful).Returns(true);
            restResponseMock.Setup(x => x.Data).Returns(_authenticationResponse);
            _restClientMock.Setup(x => x.Execute<AuthenticationResponse>(It.IsAny<IRestRequest>())).Returns(restResponseMock.Object);

            var service = new TooGoodToGoApiService(_loggerMock.Object, _apiOptions, _authenticationOptions, _restClientMock.Object);
            service.Authenticate();

            _restClientMock.Verify(x => x.Execute<AuthenticationResponse>(It.Is<IRestRequest>(r => r.Resource == $"{_apiOptions.Value.BaseUrl}{_apiOptions.Value.AuthenticateEndpoint}" && r.Method == Method.POST)));
        }

        [Test]
        public void Authenticate_Should_Returns_AuthenticationContext_With_ResponseData_When_ResponseIsSuccessful()
        {
            var restResponseMock = new Mock<IRestResponse<AuthenticationResponse>>();
            restResponseMock.Setup(x => x.IsSuccessful).Returns(true);
            restResponseMock.Setup(x => x.Data).Returns(_authenticationResponse);

            _restClientMock.Setup(x => x.Execute<AuthenticationResponse>(It.IsAny<IRestRequest>())).Returns(restResponseMock.Object);

            var service = new TooGoodToGoApiService(_loggerMock.Object, _apiOptions, _authenticationOptions, _restClientMock.Object);
            var authenticationContext = service.Authenticate();

            Assert.AreEqual(_authenticationResponse.AccessToken, authenticationContext.AccessToken);
            Assert.AreEqual(_authenticationResponse.RefreshToken, authenticationContext.RefreshToken);
            Assert.AreEqual(_authenticationResponse.StartupData.User.UserId, authenticationContext.UserId);
        }
    }
}
