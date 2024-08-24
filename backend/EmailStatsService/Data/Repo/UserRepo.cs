using EmailStatsService.Model;

namespace EmailStatsService.Data
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using EmailStatsService.DTO;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;

    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public UserRepo(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }
        public User AddUser(User user)
        {
            if (user == null)
            {
                throw new NullReferenceException();
            }

            _db.Users.Add(user);
            return user;
        }

        public User ChangeUserSettings(User user, ChangeUserSettingsRequestDTO newSettings)
        {
            user.GetWeeklyCsv = newSettings.GetWeeklyCsv;
            user.GetMonthlyCsv = newSettings.GetMonthlyCsv;
            return user;
        }

        public void DeleteUser(User user)
        {
            int userId = user.Id;
            _db.Users.Remove(user);

            var team = _db.Teams.FirstOrDefault(t => t.OwnerUserId == userId);
            if (team != null) _db.Teams.Remove(team);
        }

        public void DeleteUsers(IEnumerable<User> users)
        {
            _db.Users.RemoveRange(users);
        }

        public User FindUserWithEmail(string email)
        {
            return _db.Users.Include(u => u.Gmails).FirstOrDefault(u => u.Email == email);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _db.Users.Include(u => u.Gmails);
        }

        public string GetTokenString(User user)
        {
            string secretKey = _configuration["Jwt:Secret"];
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(secretKey);

            SecurityTokenDescriptor descriptor = new()
            {
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("email", user.Email),
                    new Claim("username", user.Username),
                    new Claim("getWeeklyCsv", user.GetWeeklyCsv.ToString()),
                    new Claim("getMonthlyCsv", user.GetMonthlyCsv.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(descriptor);
            string tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        public User GetUserById(int id)
        {
            return _db.Users.Include(u => u.Gmails).FirstOrDefault(u => u.Id == id);
        }

        public void RefreshAllUserAccessToken(Dictionary<User, AuthParamsDTO> usersWithExpiredTokens)
        {
            foreach (var kv in usersWithExpiredTokens)
            {
                var user = kv.Key;
                var newAccessToken = kv.Value.Access_token;

                user.AccessToken = newAccessToken;
                _db.Users.Update(user);
            }
        }

        public void RefreshUserAccessToken(User user, AuthParamsDTO authParamsDTO)
        {
            Console.WriteLine("new token -> " + authParamsDTO.Access_token);
            user.AccessToken = authParamsDTO.Access_token;
            user.TokenExpiryDate = DateTime.Today.AddSeconds((double)authParamsDTO.Expires_in);
        }

        public bool SaveChanges()
        {
            return _db.SaveChanges() >= 0;
        }

        public void SetUserAccessToken(User user, string accessToken)
        {
            user.AccessToken = accessToken;
            user.TokenExpiryDate = DateTime.Today.AddSeconds(3600);
        }
    }
}