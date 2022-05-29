namespace TooGoodToGo.Api.Models
{
    public class Badge
    {
        public string BadgeType { get; set; }

        public string RatingGroup { get; set; }

        public int Percentage { get; set; }

        public int UserCount { get; set; }

        public int MonthCount { get; set; }
    }
}
