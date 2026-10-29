using GpsUtil.Location;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
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

    private int _now;
    private int _maximum;
    private readonly object _lockMaximum = new();

    public RewardsService(IGpsUtil gpsUtil, IRewardCentral rewardCentral)
    {
        _gpsUtil = gpsUtil;
        _rewardsCentral = rewardCentral;
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



    public async Task CalculateABatchOfRewards(List<User> users, [Optional] bool? isNearAttraction, [Optional] bool? restrictTo5Attractions)
    {
        int batchSize = 300;
        ThreadPool.GetMinThreads(out _, out int minIoThreads);
        ThreadPool.SetMinThreads(batchSize, 100);


        Parallel.ForEach(users.Chunk(batchSize), new ParallelOptions { MaxDegreeOfParallelism = 6 }, batch =>
        {
            Parallel.ForEach(batch.AsEnumerable(), new ParallelOptions { MaxDegreeOfParallelism = 300 }, user =>
            {
                count++;
                List<VisitedLocation> userLocations = new();
                List<Attraction> attractions = new();
                Dictionary<Attraction, VisitedLocation> selectedVisitedLocationsAndAttractions = new();
                Attraction attraction;
                userLocations = user.VisitedLocations;
                attractions = _gpsUtil.GetAttractions().ToList();
                bool isNearAttraction2 = false;
                bool restrictTo5Attractions2 = false;
                if (isNearAttraction == true) { isNearAttraction2 = true; }
                if (restrictTo5Attractions == true) { restrictTo5Attractions2 = true; }
                
                if (restrictTo5Attractions2)
                {
                    attractions.Clear();
                    Dictionary<Attraction, double> tempAttractionByDistance = GetAttractionsByDistanceAscending(userLocations.LastOrDefault().Location);
                    for (int i = 0; i < 5; i++)
                    {
                        attractions.Add(tempAttractionByDistance.ElementAt(i).Key);
                    }
                }

                if (isNearAttraction2)
                {
                    for (int i1 = 0 ; i1 < attractions.Count; i1++)
                    {
                        for (int i2 = 0; i2 < userLocations.Count; i2++)
                        {
                            if (NearAttraction(userLocations[i2], attractions[i1]) && !selectedVisitedLocationsAndAttractions.ContainsKey(attractions[i1]))
                            {
                                selectedVisitedLocationsAndAttractions.Add
                                (
                                    attractions[i1], userLocations[i2]
                                );
                            }
                        }
                    } 
                }
                else
                {
                    for (int i1 = 0; i1 < attractions.Count; i1++)
                    {
                        for (int i2 = 0; i2 < userLocations.Count; i2++)
                        {
                            if (!selectedVisitedLocationsAndAttractions.ContainsKey(attractions[i1]))
                            {
                                selectedVisitedLocationsAndAttractions.Add
                                (
                                    attractions[i1], userLocations[i2]
                                );
                            }
                        }
                    }
                }

                var input = selectedVisitedLocationsAndAttractions.ToArray();

                var rewardPoints = new int[input.Length];

                Parallel.For(0, input.Length, new ParallelOptions{ MaxDegreeOfParallelism = 5 }, i =>
                {
                    rewardPoints[i] = GetRewardPoints(input[i].Key, user);
                });

                for (int i3 = 0; i3 < input.Length; i3++)
                {
                    if (!user.UserRewards.Any(r => r.Attraction.AttractionName == input.ElementAt(i3).Key.AttractionName))
                    {
                        user.AddUserReward(new UserReward(selectedVisitedLocationsAndAttractions.ElementAt(i3).Value, input.ElementAt(i3).Key, rewardPoints[i3]));
                    }
                }

            });
        });

        
        return;
    }
    public async Task CalculateRewards(User user, [Optional] bool? isNearAttraction, [Optional] bool? restrictTo5Attractions)
    {
        count++;
        List<VisitedLocation> userLocations = new();
        List<Attraction> attractions = new();
        Dictionary<Attraction, VisitedLocation> selectedVisitedLocationsAndAttractions = new();
        Attraction attraction;
        userLocations = user.VisitedLocations;
        attractions = _gpsUtil.GetAttractions().ToList();
        bool isNearAttraction2 = false;
        bool restrictTo5Attractions2 = false;
        if (isNearAttraction == true) { isNearAttraction2 = true; }
        if (restrictTo5Attractions == true) { restrictTo5Attractions2 = true; }

        if (restrictTo5Attractions2)
        {
            attractions.Clear();
            Dictionary<Attraction, double> tempAttractionByDistance = GetAttractionsByDistanceAscending(userLocations.LastOrDefault().Location);
            for (int i = 0; i < 5; i++)
            {
                attractions.Add(tempAttractionByDistance.ElementAt(i).Key);
            }
        }

        if (isNearAttraction2)
        {
            for (int i1 = 0; i1 < attractions.Count; i1++)
            {
                for (int i2 = 0; i2 < userLocations.Count; i2++)
                {
                    if (NearAttraction(userLocations[i2], attractions[i1]) && !selectedVisitedLocationsAndAttractions.ContainsKey(attractions[i1]))
                    {
                        selectedVisitedLocationsAndAttractions.Add
                        (
                            attractions[i1], userLocations[i2]
                        );
                    }
                }
            }
        }
        else
        {
            for (int i1 = 0; i1 < attractions.Count; i1++)
            {
                for (int i2 = 0; i2 < userLocations.Count; i2++)
                {
                    if (!selectedVisitedLocationsAndAttractions.ContainsKey(attractions[i1]))
                    {
                        selectedVisitedLocationsAndAttractions.Add
                        (
                            attractions[i1], userLocations[i2]
                        );
                    }
                }
            }
        }

        var input = selectedVisitedLocationsAndAttractions.ToArray();

        var rewardPoints = new int[input.Length];

        Parallel.For(0, input.Length, new ParallelOptions { MaxDegreeOfParallelism = 5 }, i =>
        {
            rewardPoints[i] = GetRewardPoints(input[i].Key, user);
        });

        for (int i3 = 0; i3 < input.Length; i3++)
        {
            if (!user.UserRewards.Any(r => r.Attraction.AttractionName == input.ElementAt(i3).Key.AttractionName))
            {
                user.AddUserReward(new UserReward(selectedVisitedLocationsAndAttractions.ElementAt(i3).Value, input.ElementAt(i3).Key, rewardPoints[i3]));
            }
        }

        return;

    }

    public bool IsWithinAttractionProximity(Attraction attraction, Locations location)
    {
        Console.WriteLine(GetDistance(attraction, location));
        return GetDistance(attraction, location) <= _attractionProximityRange;
    }

    public Dictionary<Attraction, double> GetAttractionsByDistanceAscending(Locations location)
    {
        List<Attraction> allAttractions = _gpsUtil.GetAttractions();
        int allAttractionsCountMinus1 = allAttractions.Count - 1;
        Dictionary<Attraction, double> AttractionDistanceDictionary = new ();
        Dictionary<Attraction, double> orderedByAttractionDistanceDictionary = new ();

        for (int i = 0; i <= allAttractionsCountMinus1; i++)
        {
            AttractionDistanceDictionary.Add(allAttractions[i], GetDistance(allAttractions[i], location));
        }

        foreach (KeyValuePair<Attraction, double> element in AttractionDistanceDictionary.OrderBy(key => key.Value))
        {
            orderedByAttractionDistanceDictionary.Add(element.Key, element.Value);
            Console.WriteLine($"{element.Key}" + $"{element.Value}");
        }

        return orderedByAttractionDistanceDictionary;
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
