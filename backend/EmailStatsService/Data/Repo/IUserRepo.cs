using EmailStatsService.DTO;
using EmailStatsService.Model;

namespace EmailStatsService.Data
{
    public interface IUserRepo
    {
        User AddUser(User user);
        IEnumerable<User> GetAllUsers();
        User FindUserWithEmail(string email);
        User GetUserById(int id);
        string GetTokenString(User user);
        void DeleteUsers(IEnumerable<User> users);
        void DeleteUser(User user);
        void RefreshUserAccessToken(User user, AuthParamsDTO authParamsDTO);
        void RefreshAllUserAccessToken(Dictionary<User,AuthParamsDTO> usersWithExpiredTokens);
        void SetUserAccessToken(User user, string accessToken);
        User ChangeUserSettings(User user, ChangeUserSettingsRequestDTO newSettings);
        bool SaveChanges();
    }
}