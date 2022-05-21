using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Api.Responses;
using TooGoodToGoNotifier.Configuration;
using Xunit;

namespace TooGoodToGoNotifier.Tests
{
    public class FavoriteBasketsWatcherJobTests
    {
        private readonly Mock<ILogger<FavoriteBasketsWatcherJob>> _loggerMock;
        private readonly IOptions<NotifierOptions> _notifierOptions;
        private readonly Mock<ITooGoodToGoService> _tooGoodToGoServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Context _context;
        private readonly FavoriteBasketsWatcherJob _favoriteBasketsWatcherJob;

        public FavoriteBasketsWatcherJobTests()
        {
            _loggerMock = new Mock<ILogger<FavoriteBasketsWatcherJob>>();
            _notifierOptions = Options.Create(new NotifierOptions
            {
                Recipients = new string[] { "foo@bar" }
            });
            _tooGoodToGoServiceMock = new Mock<ITooGoodToGoService>();
            _emailServiceMock = new Mock<IEmailService>();
            _context = new Context
            {
                AccessToken = "accessToken",
                UserId = 1
            };
            _favoriteBasketsWatcherJob = new FavoriteBasketsWatcherJob(_loggerMock.Object, _notifierOptions,
                _tooGoodToGoServiceMock.Object, _emailServiceMock.Object, _context);
        }

        [Fact]
        public async Task WhenBasketSeenAtleastOnceThenSeenAsOutOfStockThenSeenAgainShouldSendTwoEmails()
        {
            SetupGetBasketsResponseMock(0);
            await _favoriteBasketsWatcherJob.Invoke();
            VerifyTotalEmailSentCount(Times.Never());

            SetupGetBasketsResponseMock(1);
            await _favoriteBasketsWatcherJob.Invoke();
            await _favoriteBasketsWatcherJob.Invoke();
            VerifyTotalEmailSentCount(Times.Once());

            SetupGetBasketsResponseMock(0);
            await _favoriteBasketsWatcherJob.Invoke();
            VerifyTotalEmailSentCount(Times.Once());

            SetupGetBasketsResponseMock(1);
            await _favoriteBasketsWatcherJob.Invoke();
            VerifyTotalEmailSentCount(Times.Exactly(2));
        }

        private void VerifyTotalEmailSentCount(Times times)
        {
            _emailServiceMock.Verify(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), _notifierOptions.Value.Recipients), times);
        }

        private void SetupGetBasketsResponseMock(int availableItems)
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<Basket>
                    {
                        new Basket
                        {
                            DisplayName = "Basket",
                            ItemsAvailable = availableItems,
                            Item = new Item
                            {
                                ItemId = 1
                            }
                        }
                    }
            };

            _tooGoodToGoServiceMock.Setup(x => x.GetFavoriteBasketsAsync(_context.AccessToken, _context.UserId)).ReturnsAsync(getBasketsResponse);
        }
    }
}
