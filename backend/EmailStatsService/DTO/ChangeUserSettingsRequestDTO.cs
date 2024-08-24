namespace EmailStatsService.DTO
{
    public class ChangeUserSettingsRequestDTO
    {
        public bool GetWeeklyCsv { get; set; }
        public bool GetMonthlyCsv { get; set; }
    }
}