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
        List<OfferDTO> Get5NearestOffersDTO(User user, VisitedLocation visitedLocation);
        List<Provider> GetTripDeals(User user);
        User GetUser(string userName);
        VisitedLocation GetUserLocation(User user, [Optional] bool? isNearAttraction);
        List<UserReward> GetUserRewards(User user);
        VisitedLocation TrackUserLocation(User user, [Optional] bool? isNearAttraction, [Optional] bool? restrictTo5Attractions);
        List<VisitedLocation> TrackUserLocationInBatch(List<User> users, [Optional] bool? isNearAttraction, [Optional] bool? restrictTo5Attractions);
    }
}