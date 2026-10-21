using GpsUtil.Location;
using Microsoft.AspNetCore.Http.HttpResults;
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
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly ILogger _logger;

    public RewardsService(IGpsUtil gpsUtil, IRewardCentral rewardCentral, ILogger logger)
    {
        _gpsUtil = gpsUtil;
        _rewardsCentral =rewardCentral;
        _proximityBuffer = _defaultProximityBuffer;
        _logger = logger;
    }

    public void SetProximityBuffer(int proximityBuffer)
    {
        _proximityBuffer = proximityBuffer;
    }

    public void SetDefaultProximityBuffer()
    {
        _proximityBuffer = _defaultProximityBuffer;
    }

    public async Task CalculateRewards2(User user,[Optional] CancellationToken? ct,[Optional] bool? isNearAttraction)
    {
        var tasks = new Task[10];

        for (int i = 0; i < 10; i++)
        {
            int taskId = i;
            //tasks[i] = Task.Run(async () => await ProcessCalculateRewards(taskId));
        }

        await Task.WhenAll(tasks);
    }
    private async Task ProcessCalculateRewards(User user, [Optional] bool? isNearAttraction)
    {
        await _semaphore.WaitAsync();
        try
        {
            //var tasks = new Task[10];

            //for (int i = 0; i < 1; i++)
            //{
            //    int taskId = i;
                //tasks[i] = 
               await Task.Run(async () => await CalculateRewards(user, null, isNearAttraction));
            //}
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task CalculateRewards(User user, [Optional] CancellationToken? ct, [Optional] bool? isNearAttraction)
    {
        count++;
        List<VisitedLocation> userLocations = user.VisitedLocations;
        List<Attraction> attractions = _gpsUtil.GetAttractions().ToList();
        Dictionary<Attraction, VisitedLocation> selectedVisitedLocationsAndAttractions = new ();
        Attraction attraction;
        bool isNearAttraction2 = false;
        if (isNearAttraction == true) { isNearAttraction2 = true; }

        if(isNearAttraction2)
        { 
            for (int i1 = attractions.Count - 1 ; i1 >= 0; i1--)
            {
                for (int i2 = userLocations.Count - 1; i2 >= 0; i2--)
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
            for (int i1 = attractions.Count - 1; i1 >= 0; i1--)
            {
                for (int i2 = userLocations.Count - 1; i2 >= 0; i2--)
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

        using var internalct = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 2,
        };

        var input = selectedVisitedLocationsAndAttractions
            .Reverse()
            .ToArray();

        var rewardPoints = new int[input.Length];
        int counter = 0;
        Parallel.For(0, input.Length, i =>
        {
            rewardPoints[i] = GetRewardPoints(input[i].Key, user);
            counter ++;
        });


        for (int i3 = input.Length - 1; i3 >= 0; i3--)
        {
            if (!user.UserRewards.Any(r => r.Attraction.AttractionName == input.ElementAt(i3).Key.AttractionName))
            {
                user.AddUserReward(new UserReward(selectedVisitedLocationsAndAttractions.ElementAt(i3).Value, input.ElementAt(i3).Key, rewardPoints[i3]));
            }
        }

        //for (int i3 = selectedVisitedLocationsAndAttractions.Count - 1; i3 >= 0; i3--)
        //{
        //    if (!user.UserRewards.Any(r => r.Attraction.AttractionName == selectedVisitedLocationsAndAttractions.ElementAt(i3).Key.AttractionName))
        //    {
        //        attraction = attractions.Where(a => a.AttractionName == selectedVisitedLocationsAndAttractions.ElementAt(i3).Key.AttractionName).First();
        //        user.AddUserReward(new UserReward(selectedVisitedLocationsAndAttractions.ElementAt(i3).Value, attraction, 
        //            GetRewardPoints(selectedVisitedLocationsAndAttractions.ElementAt(i3).Key, user)));
        //    }
        //}


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
