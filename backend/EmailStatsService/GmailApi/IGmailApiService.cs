using EmailStatsService.DTO;
using EmailStatsService.Model;

namespace EmailStatsService.GmailApi
{
    public interface IGmailApiService
    {
        Task SaveGmailsForUserAfterRegister(User user);
        DetailedEmailStatisticsDTO GetDetailedUserEmailStatistics(User user, DateTime minDate, DateTime maxDate); // detailed for graphs
        SummarizedEmailStatisticsDTO GetSummarizedUserEmailStatistics(User user, DateTime minDate, DateTime maxDate);
        List<SummarizedEmailStatisticsDTO> GetTeamSummarizedStatistics(User owner, Team team, DateTime minDate, DateTime maxDate);
        Task RefreshUserGmails(User user, DateTime removeDate, DateTime getDate);
    }
}