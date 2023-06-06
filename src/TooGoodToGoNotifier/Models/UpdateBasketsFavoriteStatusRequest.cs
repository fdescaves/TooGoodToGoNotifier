namespace TooGoodToGoNotifier.Models
{
    public class UpdateBasketsFavoriteStatusRequest
    {
        public string[] BasketsIds { get; set; }

        public bool SetAsFavorite { get; set; }
    }
}
