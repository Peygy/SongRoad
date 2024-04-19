using MainApp.Models.Service;
using System.IdentityModel.Tokens.Jwt;

namespace MainApp.Services.Jwt
{
    public class JwtCheckService : IJwtCheckService
    {
        public readonly ICookieService cookieService;
        public readonly IJwtDataService jwtData;

        public JwtCheckService(ICookieService cookieService, IJwtDataService jwtData) 
        {
            this.cookieService = cookieService;
            this.jwtData = jwtData;
        }


        public bool CheckAccessToken()
        {
            cookieService.GetAccessToken();


        }
        private bool ValidAccessToken()
        {
            var handler = new JwtSecurityTokenHandler();

        }

        public bool CheckRefreshToken()
        {

        }
        public bool RevokeRefreshToken()
        {

        }
    }
}
