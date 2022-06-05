using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TooGoodToGo.Api.Interfaces;
using TooGoodToGo.Api.Models;
using TooGoodToGo.Api.Models.Responses;
using TooGoodToGoNotifier.Core;
using TooGoodToGoNotifier.Interfaces;
using TooGoodToGoNotifier.Jobs;
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
        private readonly IMemoryCache _memoryCache;
        private readonly FavoriteBasketsWatcherJob _favoriteBasketsWatcherJob;

        public FavoriteBasketsWatcherJobTests()
        {
            _loggerMock = new Mock<ILogger<FavoriteBasketsWatcherJob>>();
            _notifierOptions = Options.Create(new NotifierOptions
            {
                DefaultRecipients = new string[] { "default@recipient.com" },
                SubscribedBasketsIdByRecipients = new FilteredBaskets[]
                {
                    new FilteredBaskets
                    {
                        Recipients = new string[] { "foo@foo.com", "foo+1@foo.com" },
                        BasketIds = new string[] { "1001", "1003" }
                    },
                    new FilteredBaskets
                    {
                        Recipients = new string[] { "bar@bar.com" },
                        BasketIds = new string[] { "1002", "1003" }
                    }
                }
            });
            _tooGoodToGoServiceMock = new Mock<ITooGoodToGoService>();
            _emailServiceMock = new Mock<IEmailService>();
            _context = new Context
            {
                AccessToken = "accessToken",
                TooGoodToGoUserId = 1
            };


            // Using a real instance of IMemoryCache is easier than mocking it
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();
            _memoryCache = serviceProvider.GetService<IMemoryCache>();

            _favoriteBasketsWatcherJob = new FavoriteBasketsWatcherJob(_loggerMock.Object, _notifierOptions,
                _tooGoodToGoServiceMock.Object, _emailServiceMock.Object, _context, _memoryCache);
        }

        [Theory]
        [InlineData("1001")]
        [InlineData("1002")]
        public async Task WhenNoBasketsSeenAsAvailableShouldntSendEmailsToAnyRecipients(string notifiedBasketId)
        {
            MockGetBasketsResponse(notifiedBasketId, 0);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Theory]
        [InlineData("1001", new string[] { "foo@foo.com", "foo+1@foo.com" })]
        [InlineData("1002", new string[] { "bar@bar.com" })]
        [InlineData("1003", new string[] { "foo@foo.com", "foo+1@foo.com", "bar@bar.com" })]
        public async Task WhenBasketSeenAsAvailableOnceShouldSendEmailToSubscribedRecipients(string notifiedBasketId, string[] expectedRecipients)
        {
            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, expectedRecipients))), Times.Once);
        }

        [Fact]
        public async Task WhenBasketSeenAsAvailableOnceAndIsntFilteredShouldSendEmailToDefaultRecipients()
        {
            MockGetBasketsResponse("1000", 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, _notifierOptions.Value.DefaultRecipients))), Times.Once);
        }

        [Fact]
        public async Task WhenBasketSeenAsAvailableOnceAndIsntFilteredAndDefaultRecipientsArrayIsEmptyShouldntSendEmail()
        {
            _notifierOptions.Value.DefaultRecipients = Array.Empty<string>();
            MockGetBasketsResponse("1000", 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Theory]
        [InlineData("1001", new string[] { "foo@foo.com", "foo+1@foo.com" })]
        [InlineData("1002", new string[] { "bar@bar.com" })]
        public async Task WhenBasketSeenAsAvailableTwiceShouldOnlySendEmailOnceToSubscribedRecipients(string notifiedBasketId, string[] expectedRecipients)
        {
            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();
            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, expectedRecipients))), Times.Once);
        }

        [Theory]
        [InlineData("1001", new string[] { "foo@foo.com", "foo+1@foo.com" })]
        [InlineData("1002", new string[] { "bar@bar.com" })]
        public async Task WhenBasketSeenAsAvailableThenOutOfStockThenAvailableShouldSendEmailTwiceToSubscribedRecipients(string notifiedBasketId, string[] expectedRecipients)
        {
            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            MockGetBasketsResponse(notifiedBasketId, 0);

            await _favoriteBasketsWatcherJob.Invoke();

            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, expectedRecipients))), Times.Exactly(2));
        }

        private void MockGetBasketsResponse(string notifiedBasketId, int availableItems)
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<TgtgBasket>
                {
                    new TgtgBasket
                    {
                        ItemsAvailable = availableItems,
                        Item = new TgtgItem
                        {
                            ItemId = notifiedBasketId
                        }
                    }
                }
            };

            _tooGoodToGoServiceMock.Setup(x => x.GetFavoriteBasketsAsync(_context.AccessToken, _context.TooGoodToGoUserId)).ReturnsAsync(getBasketsResponse);
        }
    }
}
