using System.Text;
using System.Text.Json;
using System.Web;
using EmailStatsService.Data;
using EmailStatsService.DTO;
using EmailStatsService.Model;
using Microsoft.OpenApi.Models;

namespace EmailStatsService.GoogleAuth
{
    public class GoogleAuth : IGoogleAuth
    {
        public GoogleAuth(IConfiguration conf)
        {
            _conf = conf;
        }
        private readonly string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        private readonly IConfiguration _conf;

        public async Task<ExternalProfileDTO> GetUserInfo(string accessToken)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            HttpResponseMessage response = await httpClient.GetAsync(userInfoEndpoint);

            Console.WriteLine(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadFromJsonAsync<ExternalProfileDTO>();
        }

        public async Task<AuthParamsDTO> GetRefreshAndAccessTokenForUser(string code)
        {
            HttpClient httpClient = new();

            // httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
            
            var response = await httpClient.PostAsync(
                $@"https://oauth2.googleapis.com/token?&client_id={_conf["Google:ClientId"]}&client_secret={_conf["Google:ClientSecret"]}&grant_type=authorization_code&code={code}&redirect_uri={_conf["Google:OauthRedirect"]}", null
            );

            // Console.WriteLine(await response.Content.ReadAsStringAsync());

            var authParams = await response.Content.ReadFromJsonAsync<AuthParamsDTO>();

            return authParams;            
        }

        public async Task<AuthParamsDTO> GetNewAccessTokenUsingRefreshToken(string refreshToken)
        {
            HttpClient httpClient = new();

            var response = await httpClient.PostAsync(
                $@"https://oauth2.googleapis.com/token?client_id={_conf["Google:ClientId"]}&client_secret={_conf["Google:ClientSecret"]}&refresh_token={refreshToken}&grant_type=refresh_token"
                ,null
            );

            // Console.WriteLine(await response.Content.ReadAsStringAsync());

            var authParams = await response.Content.ReadFromJsonAsync<AuthParamsDTO>();

            return authParams;       
        }
    }
}