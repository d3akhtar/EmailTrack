using EmailStatsService.DTO;
using EmailStatsService.Model;

namespace EmailStatsService.GoogleAuth
{
    public interface IGoogleAuth
    {
        Task<AuthParamsDTO> GetRefreshAndAccessTokenForUser(string code);
        Task<AuthParamsDTO> GetNewAccessTokenUsingRefreshToken(string refreshToken);
        Task<ExternalProfileDTO> GetUserInfo(string accessToken);
    }
}