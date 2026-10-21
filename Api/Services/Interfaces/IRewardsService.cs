using GpsUtil.Location;
using System.Runtime.InteropServices;
using TourGuide.Users;

namespace TourGuide.Services.Interfaces
{
    public interface IRewardsService
    {
        Task CalculateRewards(User user,[Optional] CancellationToken? ct, [Optional] bool? isNearAttraction);
        Task CalculateRewards2(User user,[Optional] CancellationToken? ct, [Optional] bool? isNearAttraction);
        double GetDistance(Locations loc1, Locations loc2);
        bool IsWithinAttractionProximity(Attraction attraction, Locations location);
        Dictionary<Attraction, double> GetAttractionsByDistanceAscending(Locations location);
        void SetDefaultProximityBuffer();
        void SetProximityBuffer(int proximityBuffer);
    }
}