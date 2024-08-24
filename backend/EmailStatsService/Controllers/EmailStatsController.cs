using System.Text;
using EmailStatsService.Data;
using EmailStatsService.DTO;
using EmailStatsService.GmailApi;
using EmailStatsService.GoogleAuth;
using EmailStatsService.Helpers;
using EmailStatsService.Model;
using EmailStatsService.SyncDataServices.Smtp;
using Microsoft.AspNetCore.Mvc;

namespace EmailStatsService.Controllers
{
    [ApiController]
    [Route("/")]
    public class EmailStatsController : ControllerBase
    {
        private readonly IGmailApiService _gmailApiService;
        private readonly IUserRepo _userRepo;
        private readonly IConfiguration _conf;
        private readonly IGoogleAuth _googleAuth;

        public EmailStatsController(IGmailApiService gmailApiService, IGoogleAuth googleAuth, IUserRepo userRepo, IConfiguration conf)
        {
            _gmailApiService = gmailApiService;
            _googleAuth = googleAuth;
            _userRepo = userRepo;
            _conf = conf;
        }

        [HttpPost("external-login")]
        public async Task<ActionResult> ExternalLogin(string thirdPartyName, string accessToken, string refreshToken)
        {
            switch(thirdPartyName.ToLower())
            {
                case "google":
                    var externalUserInfo = await _googleAuth.GetUserInfo(accessToken);
                    User user = _userRepo.FindUserWithEmail(externalUserInfo.Email);

                    if (user == null)
                    {
                        user = _userRepo.AddUser(new User{
                            Username = externalUserInfo.Name,
                            Email = externalUserInfo.Email,
                            AccessToken = accessToken,
                            RefreshToken = refreshToken,
                            TokenExpiryDate = DateTime.Now.AddHours(1)
                        });

                        await _gmailApiService.SaveGmailsForUserAfterRegister(user);

                        _userRepo.SaveChanges();
                    }

                    _userRepo.SetUserAccessToken(user,accessToken);
                    _userRepo.SaveChanges();

                    var tokenString = _userRepo.GetTokenString(user);

                    return Ok(new 
                    {
                        Message = "Login successful!",
                        Token = tokenString
                    });
            }

            return BadRequest(new { Message = "This external login provider is not supporterd."});
        }

        [HttpGet("external-login")]
        public async Task<ActionResult> GetAccessAndRefreshTokens(string code, string state)
        {
            try{
                var authParams = await _googleAuth.GetRefreshAndAccessTokenForUser(code);

                var accessToken = authParams.Access_token;
                var refreshToken = authParams.Refresh_token;

                return new RedirectResult
                    (_conf["FrontEnd:LoginPage"] + "?" +  $"access_token={accessToken}" + $"&refresh_token={refreshToken}" + (string.IsNullOrEmpty(state) ? "":$"&teamJoinLink={state}"));
            }
            catch(Exception ex){
                Console.WriteLine("Error while getting access and refresh token: " + ex.Message);
                return BadRequest("Error while getting access and refresh token");
            }
        }

        [HttpDelete("delete-user")]
        public ActionResult DeleteUser()
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);

                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});

                _userRepo.DeleteUser(user);
                _userRepo.SaveChanges();

                return Ok(new {Message = "User successfully deleted"});
            }
            catch(Exception ex){
                Console.WriteLine("Error while deleting user: " + ex.Message);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        [HttpGet("detailed")]
        public ActionResult GetDetailedStatistics(DateTime minDate, DateTime maxDate)
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);

                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});

                var msgStats = _gmailApiService.GetDetailedUserEmailStatistics(user, minDate, maxDate);
                return Ok(new {
                    Message = "Success",
                    Stats = msgStats
                });
            }
            catch(Exception ex){
                Console.WriteLine("Error while getting detailed statistics for: " + ex);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        [HttpGet("summarized")]
        public ActionResult GetSummarizedStatistics(DateTime minDate, DateTime maxDate)
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);

                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});

                var msgStats = _gmailApiService.GetSummarizedUserEmailStatistics(user, minDate, maxDate);
                return Ok(new {
                    Message = "Success",
                    Stats = msgStats
                });
            }
            catch(Exception ex){
                Console.WriteLine("Error while getting summarized statistics for user: " + ex);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }

        [HttpGet("test-refresh")]
        public async Task<ActionResult> Test(int id)
        {
            var user = _userRepo.GetUserById(id);

            var previousToken = user.AccessToken;

            var authParams = await _googleAuth.GetNewAccessTokenUsingRefreshToken(user.RefreshToken);
            _userRepo.RefreshUserAccessToken(user, authParams);
            _userRepo.SaveChanges();

            return Ok(new {previousToken, NewToken = authParams.Access_token});
        }

        [HttpPut("change-settings")]
        public ActionResult ChangeUserSettings(ChangeUserSettingsRequestDTO newSettings)
        {
            try{
                var id = UserIdGetter.GetUserIdFromHttpRequest(HttpContext.Request);
            
                User user = _userRepo.GetUserById(id);

                if (user == null) return BadRequest(new {Message = "User with this id wasn't found"});

                user = _userRepo.ChangeUserSettings(user, newSettings);
                _userRepo.SaveChanges();

                return Ok(new {Message = "User settings changed successfully.", Token = _userRepo.GetTokenString(user)});
            }
            catch(Exception ex){
                Console.WriteLine("Error while updating user settings: " + ex);
                return StatusCode(500, new {Message = "Something went wrong..."});
            }
        }
    }
}