using GpsUtil.Location;
using TourGuide.Offers;
using TripPricer;

namespace TourGuide.Users;

public class User
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public DateTime LatestLocationTimestamp { get; set; }
    public List<VisitedLocation> VisitedLocations { get; } = new List<VisitedLocation>();
    public List<UserReward> UserRewards { get; } = new List<UserReward>();
    public UserPreferences UserPreferences { get; set; } = new UserPreferences();
    public List<Provider> TripDeals { get; set; } = new List<Provider>();
    public List<Offer> TempListOf5Offers { get; set; } = new List<Offer>();

    public User(Guid userId, string userName, string phoneNumber, string emailAddress)
    {
        UserId = userId;
        UserName = userName;
        PhoneNumber = phoneNumber;
        EmailAddress = emailAddress;
    }


    public void AddToVisitedLocations(VisitedLocation visitedLocation)
    {
        VisitedLocations.Add(visitedLocation);
    }

    public void ClearVisitedLocations()
    {
        VisitedLocations.Clear();
    }

    public void AddUserReward(UserReward userReward)
    {
        if (!UserRewards.Exists(r => r.Attraction.AttractionName == userReward.Attraction.AttractionName))
        {
            UserRewards.Add(userReward);
        }
    }
    public void AddOfferToTempListOf5Offers(Offer offer)
    {
        if (!TempListOf5Offers.Exists(r => r.UserReward.Attraction.AttractionName == offer.UserReward.Attraction.AttractionName))
        {
            TempListOf5Offers.Add(offer);
        }
    }
    public void ClearTempListOf5Offers()
    {
        if (TempListOf5Offers.Count <= 5)
        {
            TempListOf5Offers.Clear();
        }
    }

    public VisitedLocation GetLastVisitedLocation()
    {
        return VisitedLocations[^1];
    }
}
