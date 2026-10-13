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

public class OffersService : IOffersService
{
    private readonly ILogger _logger;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardsService _rewardsService;
    private readonly TripPricer.TripPricer _tripPricer;
    private readonly TripPricer.TripPricerTask _tripPricerTask;
    public Tracker Tracker { get; private set; }
    private readonly Dictionary<string, User> _internalUserMap = new();
    private const string TripPricerApiKey = "test-server-api-key";
    private bool _testMode = true;

    
    public Offer GenerateOffer(User user, double distance, Attraction attraction, 
        UserReward userReward, Locations locations, VisitedLocation visitedLocation)
    {
        Offer offer = new Offer
            (
                distance,
                Guid.NewGuid(),
                userReward,
                attraction,
                visitedLocation
            );
        user.AddOfferToTempListOf5Offers(offer);
        return offer;
    }


    public List<Offer> Get5NearestOffers(User user)
    {
        List<Offer> offerList = new();
        foreach(var offer in user.TempListOf5Offers)
        {
            offerList.Add(offer);
        }
        user.ClearTempListOf5Offers();
        return offerList;
    }

}
