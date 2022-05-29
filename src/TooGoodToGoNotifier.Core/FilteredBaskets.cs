namespace TooGoodToGoNotifier.Core
{
    public class FilteredBaskets
    {
        public string[] Recipients { get; set; } = System.Array.Empty<string>();

        public int[] BasketIds { get; set; } = System.Array.Empty<int>();
    }
}
