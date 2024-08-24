using EmailStatsService.Model;

namespace EmailStatsService.Data
{
    public interface ITeamRepo 
    {
        Team AddNewTeamWithOwner(User owner);
        Team GetTeamById(int id);
        Team GetTeamByJoinCode(string joinCode);
        void InviteTeamMember(Team team, User user, User owner);
        void RemoveTeamMember(Team team, User user);
        Team GetTeamByOwnerId(int ownerId);
        Team GetTeamByJoinLink(string joinLink);
        void ConfirmTeamMember(Team team, User user);
        bool TeamContainsUser(Team team, User user);
        bool SaveChanges();
    }
}