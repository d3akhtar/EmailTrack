namespace EmailStatsService.DTO
{
    public class SummarizedEmailStatisticsDTO
    {
        public string Email { get; set; }
        public int ReceivedMessages { get; set; }
        public int SentMessages { get; set; }
        public float ResponseRate { get; set; }
        public double AverageResponseTime { get; set; } // In minutes
    }
}