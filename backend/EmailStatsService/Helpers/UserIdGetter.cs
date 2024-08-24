using System.IdentityModel.Tokens.Jwt;

namespace EmailStatsService.Helpers
{
    public static class UserIdGetter
    {
        public static int GetUserIdFromHttpRequest(HttpRequest request){
            var tokenString = GetJwtTokenFromHttpRequest(request);
            // Console.WriteLine("tokenString: " + tokenString);
            if (tokenString == null) throw new ArgumentException("Error occured while decoding token.");
            var jwtPayload = GetJwtPayloadFromTokenString(tokenString);
            if (jwtPayload.TryGetValue("id", out var id)){
                return int.Parse(id.ToString());
            }
            else{
                throw new ArgumentException("Error occured while decoding token.");
            }
        }
        public static string GetJwtTokenFromHttpRequest(HttpRequest request){
            if(request.Headers.TryGetValue("Authorization", out var headerAuth)){
                // Console.WriteLine("headerAuth: " + headerAuth.ToString());
                return headerAuth.ToString().Split("Bearer ")[1];
            }
            else{
                return null;
            }
        }

        public static JwtPayload GetJwtPayloadFromTokenString(string tokenString){
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);
            
            // Console.WriteLine("token payload being returned: " + token.Payload.SerializeToJson());
            return token.Payload;
        }
    }
}