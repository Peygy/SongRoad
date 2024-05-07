using MainApp.Models.Service;
using System.Security.Claims;

namespace MainApp.Services
{
    public class CrewService : ICrewService
    {
        private readonly ICookieService cookieService;
        private readonly IJwtGenService jwtGenService;

        public CrewService(ICookieService cookieService, IJwtGenService jwtGenService) 
        {
            this.cookieService = cookieService;
            this.jwtGenService = jwtGenService;
        }


        public List<string> GetCrewRoles()
        {
            var accessToken = cookieService.GetAccessToken();
            var claims = jwtGenService.GetTokenUserClaims(accessToken).Claims;
            return claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
        }
    }
}
