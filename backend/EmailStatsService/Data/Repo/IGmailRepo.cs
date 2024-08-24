using EmailStatsService.DTO;
using EmailStatsService.Model;

namespace EmailStatsService.Data
{
    public interface IGmailRepo
    {
        void AddGmailRangeForUserFromGmailStatDTOList(List<GmailStatDTO> gmailStatDTOs, User user);
        void RemoveGmailsForUserAtCertainDate(DateTime removeDate, User user);
        bool SaveChanges();
    }
}