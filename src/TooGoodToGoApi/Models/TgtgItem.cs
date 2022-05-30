namespace TooGoodToGo.Api.Models
{
    public class TgtgItem
    {
        public string ItemId { get; set; }

        public TgtgSalesTaxes[] SalesTaxes { get; set; }

        public TgtgTaxAmount TaxAmount { get; set; }

        public TgtgTaxAmount PriceExcludingTaxes { get; set; }

        public TgtgTaxAmount PriceIncludingTaxes { get; set; }

        public TgtgTaxAmount ValueExcludingTaxes { get; set; }

        public TgtgTaxAmount ValueIncludingTaxes { get; set; }

        public string TaxationPolicy { get; set; }

        public bool ShowSalesTaxes { get; set; }

        public TgtgCoverPicture CoverPicture { get; set; }

        public TgtgLogoPicture LogoPicture { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string FoodHandlingInstructions { get; set; }

        public bool CanUserSupplyPackaging { get; set; }

        public string PackagingOption { get; set; }

        public string CollectionInfo { get; set; }

        public object[] DietCategories { get; set; }

        public string ItemCategory { get; set; }

        public TgtgBadge[] Badges { get; set; }

        public string[] PositiveRatingReasons { get; set; }

        public TgtgRating AverageOverallRating { get; set; }

        public int FavoriteCount { get; set; }

        public bool Buffet { get; set; }
    }
}
