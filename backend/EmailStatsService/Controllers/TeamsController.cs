using EmailStatsService.Data;
using EmailStatsService.DTO;
using EmailStatsService.GmailApi;
using EmailStatsService.Helpers;
using EmailStatsService.Model;
using EmailStatsService.SyncDataServices.Smtp;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Engines;

namespace EmailStatsService.Controllers
{
    [ApiController]
    [Route("/team")]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamRepo _teamRepo;
        private readonly IUserRepo _userRepo;
        private readonly IGmailApiService _gmailApiService;

        public TeamsController(ITeamRepo teamRepo, IUserRepo userRepo, IGmailApiService gmailApiService)
        {
            _teamRepo = teamRepo;
            _userRepo = userRepo;
            _gmailApiService = gmailApiService;
        }

        [HttpGet]
        public ActionResult GetTeam()
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);
                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});
                
                Team team = _teamRepo.GetTeamByOwnerId(user.Id);
                if (team == null) return BadRequest(new {Message = "User doesn't have a team yet."});

                var teamMembers = new List<TeamMemberDTO>(){ user.GetTeamMemberDTO() };
                teamMembers.AddRange(team.Users.Select(u => u.GetTeamMemberDTO()).ToList());

                return Ok(new {
                    Message = "Team member info retrieved!",
                    Team = teamMembers
                });
            }
            catch(Exception ex){
                Console.WriteLine("Error while inviting user to team: " + ex.Message);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        [HttpPost("invite")]
        public ActionResult InviteToTeam([FromQuery]string email)
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);
                User userToInvite = _userRepo.FindUserWithEmail(email);
                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});
                if (userToInvite == null) return BadRequest(new {Message = "Cannot find user with given email to invite."});
                if (user.Id == userToInvite.Id) return BadRequest(new {Message = "You can't invite the owner."});
                
                Team team = _teamRepo.GetTeamByOwnerId(user.Id);
                if (team == null) return BadRequest(new {Message = "User doesn't have a team yet."});
                if (user.Team != null) return BadRequest(new {Message = "User is already part of another team."});
                if (_teamRepo.TeamContainsUser(team,userToInvite)) return BadRequest(new {Message = "User is already in the team."});

                _teamRepo.InviteTeamMember(team,userToInvite,user);
                _teamRepo.SaveChanges();

                return Ok(new {Message = "User has been invited!"});
            }
            catch(Exception ex){
                Console.WriteLine("Error while inviting user to team: " + ex.Message);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        [HttpPost("create")]
        public ActionResult CreateTeam()
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);

                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});
                if (_teamRepo.GetTeamByOwnerId(user.Id) != null) return BadRequest(new {Message = "User already has a team."});

                _teamRepo.AddNewTeamWithOwner(user);
                _teamRepo.SaveChanges();

                return Ok(new {Message = "Team created successfully!"});
            }
            catch(Exception ex){
                Console.WriteLine("Error while inviting user to team: " + ex.Message);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        [HttpPost("join")]
        public ActionResult JoinTeam([FromQuery]string teamJoinCode)
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);
                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});

                Team team = _teamRepo.GetTeamByJoinCode(teamJoinCode);
                if (team == null) return BadRequest(new {Message = "Team with this join code wasn't found."});
                if (!_teamRepo.TeamContainsUser(team,user)) return BadRequest(new {Message = "User wasn't invited to this team."});

                _teamRepo.ConfirmTeamMember(team,user);
                _teamRepo.SaveChanges();

                return Ok(new {Message = "User is now part of team!"});
            }
            catch(ArgumentException ex)
            {
                Console.WriteLine("Error while joining team: " + ex.Message);
                return BadRequest(new {Message = ParseErrorMessage(ex.Message)});
            }
            catch(Exception ex){
                Console.WriteLine("Error while joining team: " + ex.Message);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        [HttpGet("stats")]
        public ActionResult GetUserTeamSummarizedStats(DateTime minDate, DateTime maxDate)
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);
                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});

                Team team = _teamRepo.GetTeamByOwnerId(user.Id);                
                if (team == null) return BadRequest(new {Message = "User doesn't have a team."});

                var teamStats = _gmailApiService.GetTeamSummarizedStatistics(user, team, minDate, maxDate);

                return Ok(new {
                    Message = "Team stats retrieved!",
                    Stats = teamStats
                });
            }
            catch(Exception ex){
                Console.WriteLine("Error while inviting user to team: " + ex.Message);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        [HttpDelete]
        public ActionResult DeleteTeamMember(string email)
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);
                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});

                Team team = _teamRepo.GetTeamByOwnerId(user.Id);                
                if (team == null) return BadRequest(new {Message = "User doesn't have a team."});

                User userToRemove = _userRepo.FindUserWithEmail(email);
                if (userToRemove == null) return BadRequest(new { Message = "Can't find user with this email."});
                if (!_teamRepo.TeamContainsUser(team,userToRemove)) return BadRequest(new {Message = "User is not part of the team."});

                _teamRepo.RemoveTeamMember(team,userToRemove);
                _teamRepo.SaveChanges();

                return Ok(new {
                    Message = "Team member removed successfully!",
                });
            }
            catch(Exception ex){
                Console.WriteLine("Error while removing user from team: " + ex.Message);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        [HttpPost("decline")]
        public ActionResult DeclineInvitation([FromQuery]string teamJoinLink)
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);
                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});

                Team team = _teamRepo.GetTeamByJoinLink(teamJoinLink);                
                if (team == null) return BadRequest(new {Message = "Team not found."});

                _teamRepo.RemoveTeamMember(team,user);
                _teamRepo.SaveChanges();

                return Ok(new {
                    Message = "Invitation declined successfully!",
                });
            }
            catch(Exception ex){
                Console.WriteLine("Error while declining team invitation: " + ex.Message);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        private string ParseErrorMessage(string errorMsg)
        {
            int firstApostropheIndex = errorMsg.IndexOf('\u0027');
            int secondApostropheIndex = errorMsg.IndexOf('\u0027', firstApostropheIndex+1);

            return errorMsg.Substring(firstApostropheIndex+1, secondApostropheIndex - 1 - firstApostropheIndex);
        }
    }
}