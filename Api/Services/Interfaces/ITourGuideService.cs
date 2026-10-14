using GpsUtil.Location;
using System.Runtime.InteropServices;
using TourGuide.Offers;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services.Interfaces
{
    public interface ITourGuideService
    {
        Tracker Tracker { get; }

        void AddUser(User user);
        List<User> GetAllUsers();
        List<Attraction> GetNearByAttractions(VisitedLocation visitedLocation);
        List<Offer> Get5NearestOffers(User user, VisitedLocation visitedLocation);
        List<Provider> GetTripDeals(User user);
        User GetUser(string userName);
        VisitedLocation GetUserLocation(User user, [Optional] bool? isNearAttraction);
        List<UserReward> GetUserRewards(User user);
        VisitedLocation TrackUserLocation(User user, [Optional] bool? isNearAttraction);
    }
}