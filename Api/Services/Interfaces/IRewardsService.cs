using GpsUtil.Location;
using System.Runtime.InteropServices;
using TourGuide.Users;

namespace TourGuide.Services.Interfaces
{
    public interface IRewardsService
    {
        void CalculateRewards(User user, [Optional] bool? isNearAttraction);
        double GetDistance(Locations loc1, Locations loc2);
        bool IsWithinAttractionProximity(Attraction attraction, Locations location);
        Dictionary<Attraction, double> GetAttractionsByDistanceAscending(Locations location);
        void SetDefaultProximityBuffer();
        void SetProximityBuffer(int proximityBuffer);
    }
}