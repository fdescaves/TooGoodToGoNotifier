using System;

namespace TooGoodToGoNotifier.Core.Options
{
    public class FilteredBaskets
    {
        public string[] Recipients { get; set; } = Array.Empty<string>();

        public string[] BasketIds { get; set; } = Array.Empty<string>();
    }
}
