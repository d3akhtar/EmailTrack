using System.Data.Common;
using EmailStatsService.Model;
using EmailStatsService.SyncDataServices.Smtp;
using Microsoft.EntityFrameworkCore;

namespace EmailStatsService.Data
{
    public class TeamRepo : ITeamRepo 
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _conf;

        public TeamRepo(AppDbContext db, IEmailService emailService, IConfiguration conf)
        {
            _db = db;
            _emailService = emailService;
            _conf = conf;
        }

        public Team AddNewTeamWithOwner(User owner)
        {
            Team team = new()
            {
                OwnerUserId = owner.Id,
                TeamJoinCode = Guid.NewGuid().ToString() + _db.Teams.Count().ToString(),
                Users = new List<User>()
            };
            _db.Teams.Add(team);
            return team;
        }

        public void ConfirmTeamMember(Team team, User user)
        {
            if (team.Users.Contains(user)){
                if (!user.InvitePending) throw new ArgumentNullException("User is already part of this team.");
                user.InvitePending = false;
            }
            else throw new ArgumentException("Invited user not part of team");
        }

        public Team GetTeamById(int id) => _db.Teams.Include(t => t.Users).ThenInclude(u => u.Gmails).FirstOrDefault(t => t.Id == id);
        public Team GetTeamByJoinCode(string joinCode) => _db.Teams.Include(t => t.Users).ThenInclude(u => u.Gmails).FirstOrDefault(t => t.TeamJoinCode == joinCode);
        public Team GetTeamByOwnerId(int ownerId) => _db.Teams.Include(t => t.Users).ThenInclude(u => u.Gmails).FirstOrDefault(t => t.OwnerUserId == ownerId);
        
        // Send email to user as well
        public void InviteTeamMember(Team team, User user, User owner)
        {
            team.Users.Add(user);
            user.InvitePending = true;

            var frontendTeamJoinLink = $"{_conf["FrontEnd:LoginPage"]}?teamJoinLink={team.TeamJoinCode}";
            var inviteEmailBody = 
            $@" <h3>EmailTrack Team Invitation</h3>
                <p>A user with the email of {owner.Email} invited you to their EmailTrack team. Click <a href='{frontendTeamJoinLink}'>here</a> to join.</p>
            ";

            _emailService.SendEmail("EmailTrack Team Invite", inviteEmailBody, user.Email);
        }
        public void RemoveTeamMember(Team team, User user) {
            team.Users.Remove(user);
            user.TeamId = null;
            user.InvitePending = false;
        }

        public bool TeamContainsUser(Team team, User user)
        {
            foreach (var member in team.Users){
                if (member.Id == user.Id) return true;
            }

            return false;
        }
        
        public bool SaveChanges() => _db.SaveChanges() >= 0;

        public Team GetTeamByJoinLink(string joinLink) => _db.Teams.Include(t => t.Users).ThenInclude(u => u.Gmails).FirstOrDefault(t => t.TeamJoinCode == joinLink);
    }
}