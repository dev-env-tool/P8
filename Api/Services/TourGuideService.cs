using GpsUtil.Location;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Offers;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services;

public class TourGuideService : ITourGuideService
{
    private readonly ILogger _logger;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardsService _rewardsService;
    private readonly IOffersService _offerService;
    private readonly TripPricer.TripPricer _tripPricer;
    private readonly TripPricer.TripPricerTask _tripPricerTask;
    public Tracker Tracker { get; private set; }
    private readonly Dictionary<string, User> _internalUserMap = new();
    private const string TripPricerApiKey = "test-server-api-key";
    private bool _testMode = true;

    public TourGuideService(ILogger<TourGuideService> logger, IGpsUtil gpsUtil, IRewardsService rewardsService, IOffersService offerService, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _tripPricer = new();
        _tripPricerTask = new("", new Guid(), 0, 0, 0, 0);
        _gpsUtil = gpsUtil;
        _rewardsService = rewardsService;
        _offerService = offerService;

        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        if (_testMode)
        {
            _logger.LogInformation("TestMode enabled");
            _logger.LogDebug("Initializing users");
            InitializeInternalUsers();
            _logger.LogDebug("Finished initializing users");
        }

        var trackerLogger = loggerFactory.CreateLogger<Tracker>();

        Tracker = new Tracker(this, trackerLogger);
        AddShutDownHook();
    }

    public List<UserReward> GetUserRewards(User user)
    {
        return user.UserRewards;
    }

    public VisitedLocation GetUserLocation(User user)
    {
        return user.VisitedLocations.Any() ? user.GetLastVisitedLocation() : TrackUserLocation(user);
    }

    public User GetUser(string userName)
    {
        return _internalUserMap.ContainsKey(userName) ? _internalUserMap[userName] : null;
    }

    public List<User> GetAllUsers()
    {
        return _internalUserMap.Values.ToList();
    }

    public void AddUser(User user)
    {
        if (!_internalUserMap.ContainsKey(user.UserName))
        {
            _internalUserMap.Add(user.UserName, user);
        }
    }


    public List<Provider> GetTripDeals(User user)
    {
        int cumulativeRewardPoints = user.UserRewards.Sum(i => i.RewardPoints);
        Task<List<Provider>> providers = Task.Run(() => _tripPricerTask.ExecuteAsync(TripPricerApiKey, user.UserId,
            user.UserPreferences.NumberOfAdults, user.UserPreferences.NumberOfChildren,
            user.UserPreferences.TripDuration, cumulativeRewardPoints).GetAwaiter().GetResult());
        user.TripDeals = providers.Result.ToList();
        return providers.Result.ToList();
    }

    public VisitedLocation TrackUserLocation(User user)
    {
        VisitedLocation visitedLocation = _gpsUtil.GetUserLocation(user.UserId);
        user.AddToVisitedLocations(visitedLocation);
        _rewardsService.CalculateRewards(user);
        return visitedLocation;
    }

    public List<Attraction> GetNearByAttractions(VisitedLocation visitedLocation)
    {
        List<Attraction> nearbyAttractions = new ();
        foreach (var attraction in _gpsUtil.GetAttractions())
        {
            if (_rewardsService.IsWithinAttractionProximity(attraction, visitedLocation.Location))
            {
                nearbyAttractions.Add(attraction);
            }
        }

        return nearbyAttractions;
    }

    public List<Offer> Get5NearestOffers(User user, VisitedLocation visitedLocation)
    {
        Dictionary<Attraction, double> NearestAttractionsAndDistance = Get5NearestAttractions(user, visitedLocation);
        List<UserReward> rewards = new List<UserReward>();
        UserReward bufferUserReward;
        foreach (var attractionDistance in NearestAttractionsAndDistance)
        {
            bufferUserReward = rewards.Where(r => r.Attraction.AttractionName == attractionDistance.Key.AttractionName).First();
            _offerService.GenerateOffer(user, attractionDistance.Value, attractionDistance.Key, bufferUserReward, bufferUserReward.VisitedLocation.Location,visitedLocation);
        }
        return _offerService.Get5NearestOffers(user);
    }

    private Dictionary<Attraction, double> Get5NearestAttractions(User user, VisitedLocation visitedLocation)
    {
        Dictionary<Attraction, double> AttractionDistanceDictionary = _rewardsService.GetAttractionsByDistanceAscending(visitedLocation.Location);
        Dictionary<Attraction, double> listOf5NearestAttractions = new();

        for (int i = 0; i <= 4; i++)
        {
            listOf5NearestAttractions.Add(
                AttractionDistanceDictionary.ElementAt(i).Key,
                AttractionDistanceDictionary.ElementAt(i).Value);
        }

        return listOf5NearestAttractions;
    }


    private void AddShutDownHook()
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => Tracker.StopTracking();
    }

    /**********************************************************************************
    * 
    * Methods Below: For Internal Testing
    * 
    **********************************************************************************/

    private void InitializeInternalUsers()
    {
        for (int i = 0; i < InternalTestHelper.GetInternalUserNumber(); i++)
        {
            var userName = $"internalUser{i}";
            var user = new User(Guid.NewGuid(), userName, "000", $"{userName}@tourGuide.com");
            GenerateUserLocationHistory(user);
            _internalUserMap.Add(userName, user);
        }

        _logger.LogDebug($"Created {InternalTestHelper.GetInternalUserNumber()} internal test users.");
    }

    private void GenerateUserLocationHistory(User user)
    {
        for (int i = 0; i < 3; i++)
        {
            var visitedLocation = new VisitedLocation(user.UserId, new Locations(GenerateRandomLatitude(), GenerateRandomLongitude()), GetRandomTime());
            user.AddToVisitedLocations(visitedLocation);
        }
    }

    private static readonly Random random = new Random();

    private double GenerateRandomLongitude()
    {
        return new Random().NextDouble() * (180 - (-180)) + (-180);
    }

    private double GenerateRandomLatitude()
    {
        return new Random().NextDouble() * (90 - (-90)) + (-90);
    }

    private DateTime GetRandomTime()
    {
        return DateTime.UtcNow.AddDays(-new Random().Next(30));
    }
}
