using EmailStatsService.DTO;
using EmailStatsService.Model;

namespace EmailStatsService.Helpers
{
    public static class ResponseTimeCalculator
    {
        public static double GetMinutesBetweenTwoGmails(GmailStatDTO a, GmailStatDTO b)
            => 
            (
                DateTimeOffset.FromUnixTimeMilliseconds(a.InternalDate) - 
                DateTimeOffset.FromUnixTimeMilliseconds(b.InternalDate)
            ).TotalMinutes;
        
        public static double GetMinutesBetweenTwoGmails(Gmail a, Gmail b)
            => 
            (
                DateTimeOffset.FromUnixTimeMilliseconds(a.InternalDate) - 
                DateTimeOffset.FromUnixTimeMilliseconds(b.InternalDate)
            ).TotalMinutes;
    }
}