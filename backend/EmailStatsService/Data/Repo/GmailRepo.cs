using EmailStatsService.DTO;
using EmailStatsService.Model;

namespace EmailStatsService.Data
{
    public class GmailRepo : IGmailRepo
    {
        private readonly AppDbContext _db;

        public GmailRepo(AppDbContext db)
        {
            _db = db;
        }
        public void AddGmailRangeForUserFromGmailStatDTOList(List<GmailStatDTO> gmailStatDTOs, User user)
        {
            // First and last gmail was duplicated for some reason
            HashSet<string> seenIds = new();
            foreach (var g in gmailStatDTOs)
            {
                if (!seenIds.Contains(g.Id)){
                    seenIds.Add(g.Id);
                    var gmail = g.ConvertToGmailModelForUser(user);
                    _db.Gmails.Add(gmail);
                    user.Gmails.Add(gmail);
                }
            }
        }

        public void RemoveGmailsForUserAtCertainDate(DateTime removeDate, User user)
        {
            foreach (var g in user.Gmails)
            {
                if (g.WithinDateRange(removeDate)){
                    _db.Gmails.Remove(g);
                    user.Gmails.Remove(g);
                }
            }
        }

        public bool SaveChanges() => _db.SaveChanges() >= 0;
    }
}