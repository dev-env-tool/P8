using GpsUtil.Location;
using TourGuide.Offers;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services.Interfaces
{
    public interface IOffersService
    {
        Offer GenerateOffer(User user, double distance, Attraction attraction,
            UserReward userReward, Locations locations, VisitedLocation visitedLocation);
        public List<Offer> Get5NearestOffers(User user);
    }
}