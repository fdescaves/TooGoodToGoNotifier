using System;

namespace TooGoodToGoNotifier.Core
{
    public class FilteredBaskets
    {
        public string[] Recipients { get; set; } = Array.Empty<string>();

        public string[] BasketIds { get; set; } = Array.Empty<string>();
    }
}
