using GpsUtil.Location;
using TourGuide.Users;

namespace TourGuide.Offers
{
    public class OfferDTO
    {
        public string AttractionName { get; }
        public double AttractionLatitude { get; }
        public double AttractionLongitude { get; }

        public double UserLocationLatitude { get; }
        public double UserLocationLongitude { get; }

        public double Distance { get; }
        public int RewardPoints { get; }
        public Guid OfferId { get; }

        public OfferDTO(string attractionName, double attractionLattitude, double attractionLongitude,
            double userLocationLattitude, double userLocationLongitude, double distance, int rewardPoints, Guid offerId)
        {
            AttractionName = attractionName;
            AttractionLatitude = attractionLattitude;
            AttractionLongitude = attractionLongitude;
            UserLocationLatitude = userLocationLattitude;
            UserLocationLongitude = userLocationLongitude;
            Distance = distance;
            RewardPoints = rewardPoints;
            OfferId = Guid.NewGuid();
        }

    }
}
