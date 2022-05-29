namespace TooGoodToGo.Api.Models
{
    public class Item
    {
        public int ItemId { get; set; }

        public SalesTaxes[] SalesTaxes { get; set; }

        public TaxAmount TaxAmount { get; set; }

        public TaxAmount PriceExcludingTaxes { get; set; }

        public TaxAmount PriceIncludingTaxes { get; set; }

        public TaxAmount ValueExcludingTaxes { get; set; }

        public TaxAmount ValueIncludingTaxes { get; set; }

        public string TaxationPolicy { get; set; }

        public bool ShowSalesTaxes { get; set; }

        public CoverPicture CoverPicture { get; set; }

        public LogoPicture LogoPicture { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string FoodHandlingInstructions { get; set; }

        public bool CanUserSupplyPackaging { get; set; }

        public string PackagingOption { get; set; }

        public string CollectionInfo { get; set; }

        public object[] DietCategories { get; set; }

        public string ItemCategory { get; set; }

        public Badge[] Badges { get; set; }

        public string[] PositiveRatingReasons { get; set; }

        public Rating AverageOverallRating { get; set; }

        public int FavoriteCount { get; set; }

        public bool Buffet { get; set; }
    }
}
