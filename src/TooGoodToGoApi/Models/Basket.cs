using System;

namespace TooGoodToGo.Api.Models
{
    public class Basket
    {
        public Item Item { get; set; }

        public Store Store { get; set; }

        public string DisplayName { get; set; }

        public PickupInterval PickupInterval { get; set; }

        public PickupLocation PickupLocation { get; set; }

        public DateTime PurchaseEnd { get; set; }

        public int ItemsAvailable { get; set; }

        public DateTime SoldOutAt { get; set; }

        public float Distance { get; set; }

        public bool Favorite { get; set; }

        public bool InSalesWindow { get; set; }

        public bool NewItem { get; set; }
    }
}
