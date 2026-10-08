using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPricer;

public class TripPricerTask : Task<List<Provider>>
{
    private readonly Guid _attractionId;
    private readonly string _apiKey;
    private readonly int _adults;
    private readonly int _children;
    private readonly int _nightsStay;
    private readonly int _rewardPoints;

    public TripPricerTask(string apiKey, Guid attractionId, int adults, int children, int nightsStay, int rewardPoints)
        : base(() => new TripPricer().GetPrice(apiKey, attractionId, adults, children, nightsStay, rewardPoints))
    {
        _apiKey = apiKey;
        _attractionId = attractionId;
        _adults = adults;
        _children = children;
        _nightsStay = nightsStay;
        _rewardPoints = rewardPoints;
    }
    public async Task<List<Provider>> ExecuteAsync(string apiKey, Guid attractionId, int adults, int children, int nightsStay, int rewardPoints)
    {
        var tripPricer = new TripPricer();
        return await Task.Run(() => tripPricer.GetPrice(_apiKey, _attractionId, _adults, _children, _nightsStay, _rewardPoints));
    }
}
