using System;
using System.Collections.Generic;
using System.Linq;
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
                DefaultRecipients = new string[] { "default@recipient.com" },
                SubscribedRecipientsByBasketId = new Dictionary<string, string[]>
                {
                    {
                        "1001",
                        new string[] { "foo@foo.com", "foo+1@foo.com" }
                    },
                    {
                        "1002",
                        new string[] { "bar@bar.com" }
                    },
                    {
                        "1003",
                        Array.Empty<string>()
                    }
                }
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

        [Theory]
        [InlineData(1001)]
        [InlineData(1002)]
        public async Task WhenNoBasketsSeenAsAvailableShouldntSendEmailsToAnyRecipients(int notifiedBasketId)
        {
            MockGetBasketsResponse(notifiedBasketId, 0);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Theory]
        [InlineData(1001, new string[] { "foo@foo.com", "foo+1@foo.com" })]
        [InlineData(1002, new string[] { "bar@bar.com" })]
        public async Task WhenBasketSeenAsAvailableOnceShouldSendEmailToSubscribedRecipients(int notifiedBasketId, string[] expectedRecipients)
        {
            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, expectedRecipients))), Times.Once);
        }

        [Fact]
        public async Task WhenBasketSeenAsAvailableOnceAndIsntFilteredShouldSendEmailToDefaultRecipients()
        {
            MockGetBasketsResponse(1000, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, _notifierOptions.Value.DefaultRecipients))), Times.Once);
        }

        [Fact]
        public async Task WhenBasketSeenAsAvailableOnceAndRecipientsArrayIsEmptyShouldntSendEmail()
        {
            MockGetBasketsResponse(1003, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Fact]
        public async Task WhenBasketSeenAsAvailableOnceAndIsntFilteredAndDefaultRecipientsArrayIsEmptyShouldntSendEmail()
        {
            _notifierOptions.Value.DefaultRecipients = Array.Empty<string>();
            MockGetBasketsResponse(1000, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Theory]
        [InlineData(1001, new string[] { "foo@foo.com", "foo+1@foo.com" })]
        [InlineData(1002, new string[] { "bar@bar.com" })]
        public async Task WhenBasketSeenAsAvailableTwiceShouldOnlySendEmailOnceToSubscribedRecipients(int notifiedBasketId, string[] expectedRecipients)
        {
            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();
            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, expectedRecipients))), Times.Once);
        }

        [Theory]
        [InlineData(1001, new string[] { "foo@foo.com", "foo+1@foo.com" })]
        [InlineData(1002, new string[] { "bar@bar.com" })]
        public async Task WhenBasketSeenAsAvailableThenOutOfStockThenAvailableShouldSendEmailTwiceToSubscribedRecipients(int notifiedBasketId, string[] expectedRecipients)
        {
            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            MockGetBasketsResponse(notifiedBasketId, 0);

            await _favoriteBasketsWatcherJob.Invoke();

            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, expectedRecipients))), Times.Exactly(2));
        }

        private void MockGetBasketsResponse(int notifiedBasketId, int availableItems)
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<Basket>
                {
                    new Basket
                    {
                        ItemsAvailable = availableItems,
                        Item = new Item
                        {
                            ItemId = notifiedBasketId
                        }
                    }
                }
            };

            _tooGoodToGoServiceMock.Setup(x => x.GetFavoriteBasketsAsync(_context.AccessToken, _context.UserId)).ReturnsAsync(getBasketsResponse);
        }
    }
}
