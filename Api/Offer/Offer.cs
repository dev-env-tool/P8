using GpsUtil.Location;
using TourGuide.Users;

namespace TourGuide.Offers
{
    public class Offer
    {
        public double Distance { get; }
        public Guid OfferId { get; }
        public UserReward UserReward { get; set; }
        public VisitedLocation VisitedLocation { get; }

        public Offer(double distance, Guid offerId, UserReward userReward, VisitedLocation visitedLocation)
        {
            Distance = distance;
            OfferId = Guid.NewGuid();
            UserReward = userReward;
            VisitedLocation = visitedLocation;
        }

    }
}
