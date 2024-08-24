namespace EmailStatsService.DTO
{
    public class AuthParamsDTO
    {
        public string? Access_token { get; set; }
        public string? Refresh_token { get; set; }
        public int? Expires_in { get; set; }
    }
}