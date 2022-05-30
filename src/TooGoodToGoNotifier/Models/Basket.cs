namespace TooGoodToGoNotifier.Models
{
    public class Basket
    {
        public string BasketId { get; set; }

        public string Name { get; set; }

        public string StoreId { get; set; }

        public Store Store { get; set; }
    }
}
