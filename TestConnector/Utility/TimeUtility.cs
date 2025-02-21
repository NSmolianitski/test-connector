namespace TestConnector.Utility;

public static class TimeUtility
{
    public static string GetClosestInterval(int periodInSec)
    {
        var intervalData = new (int Seconds, string Interval)[]
        {
            (60, "1m"), (300, "5m"), (900, "15m"), (1800, "30m"),
            (3600, "1h"), (10800, "3h"), (21600, "6h"), (43200, "12h"),
            (86400, "1D"), (604800, "1W"), (1209600, "14D"), (2592000, "1M")
        };

        foreach (var (sec, interval) in intervalData)
        {
            if (periodInSec <= sec)
                return interval;
        }
        
        return intervalData[^1].Interval;
    }
}