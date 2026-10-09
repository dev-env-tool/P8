using GpsUtil.Location;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;

namespace TourGuide.Services;

public class RewardsService : IRewardsService
{
    private const double StatuteMilesPerNauticalMile = 1.15077945;
    private readonly int _defaultProximityBuffer = 10;
    private int _proximityBuffer;
    private readonly int _attractionProximityRange = 200;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardCentral _rewardsCentral;
    private static int count = 0;

    public RewardsService(IGpsUtil gpsUtil, IRewardCentral rewardCentral)
    {
        _gpsUtil = gpsUtil;
        _rewardsCentral =rewardCentral;
        _proximityBuffer = _defaultProximityBuffer;
    }

    public void SetProximityBuffer(int proximityBuffer)
    {
        _proximityBuffer = proximityBuffer;
    }

    public void SetDefaultProximityBuffer()
    {
        _proximityBuffer = _defaultProximityBuffer;
    }

    public void CalculateRewards(User user)
    {
        count++;
        List<VisitedLocation> userLocations = user.VisitedLocations;
        List<Attraction> attractions = _gpsUtil.GetAttractions().ToList();


        Dictionary<string, VisitedLocation> selectedVisitedLocationsAndAttractions = new Dictionary<string, VisitedLocation>();
        List<Attraction> selectedAttractions = new List<Attraction>();
        List<UserReward> UserRewardsToBeCreated = new List<UserReward>();
        Attraction attraction;
        //foreach (var visitedLocation in userLocations)
        //{
        //    foreach (var attraction in attractions)
        //    {
        //        if (!user.UserRewards.Any(r => r.Attraction.AttractionName == attraction.AttractionName))
        //        {
        //            if (NearAttraction(visitedLocation, attraction))
        //            {
        //                user.AddUserReward(new UserReward(visitedLocation, attraction, GetRewardPoints(attraction, user)));
        //            }
        //        }
        //    }
        //}

        //for (int i = attractions.Count - 1; i >= 0; i--)
        //{
        //    if (!user.UserRewards.Any(r => r.Attraction.AttractionName == attractions[i].AttractionName))
        //    {
        //        for (int i2 = userLocations.Count - 1; i2 >= 0; i2--)
        //        {
        //            if (NearAttraction(userLocations[i2], attractions[i]))
        //            {
        //                user.AddUserReward(new UserReward(userLocations[i2], attractions[i], GetRewardPoints(attractions[i], user)));
        //            }
        //        }
        //    }
        //}

        for (int i1 = attractions.Count - 1 ; i1 >= 0; i1--)
        {
            for (int i2 = userLocations.Count - 1; i2 >= 0; i2--)
            {
                if (NearAttraction(userLocations[i2], attractions[i1]) && !selectedVisitedLocationsAndAttractions.ContainsKey(attractions[i1].AttractionName))
                {
                    selectedVisitedLocationsAndAttractions.Add
                    (
                        attractions[i1].AttractionName, userLocations[i2]
                    );  
                }
            }
        }

        for (int i3 = selectedVisitedLocationsAndAttractions.Count - 1; i3 >= 0; i3--)
        {
            if (!user.UserRewards.Any(r => r.Attraction.AttractionName == selectedVisitedLocationsAndAttractions.ElementAt(i3).Key))
            {
                attraction = attractions.Where(a => a.AttractionName == selectedVisitedLocationsAndAttractions.ElementAt(i3).Key).First();
                user.AddUserReward(new UserReward(selectedVisitedLocationsAndAttractions.ElementAt(i3).Value, attraction, GetRewardPoints(attraction, user)));
            }
        }


    }

    public bool IsWithinAttractionProximity(Attraction attraction, Locations location)
    {
        Console.WriteLine(GetDistance(attraction, location));
        return GetDistance(attraction, location) <= _attractionProximityRange;
    }

    private bool NearAttraction(VisitedLocation visitedLocation, Attraction attraction)
    {
        return GetDistance(attraction, visitedLocation.Location) <= _proximityBuffer;
    }

    private int GetRewardPoints(Attraction attraction, User user)
    {
        return _rewardsCentral.GetAttractionRewardPoints(attraction.AttractionId, user.UserId);
    }

    public double GetDistance(Locations loc1, Locations loc2)
    {
        double lat1 = Math.PI * loc1.Latitude / 180.0;
        double lon1 = Math.PI * loc1.Longitude / 180.0;
        double lat2 = Math.PI * loc2.Latitude / 180.0;
        double lon2 = Math.PI * loc2.Longitude / 180.0;

        double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2)
                                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2));

        double nauticalMiles = 60.0 * angle * 180.0 / Math.PI;
        return StatuteMilesPerNauticalMile * nauticalMiles;
    }
}
