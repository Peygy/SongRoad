using System.Security.Claims;

namespace MainApp.Models
{
    public interface IJwtService
    {
        void GenerateTokens(UserModel user, List<Claim> authClaims);

        void CheckAccessToken(string accessToken);

        void CheckRefreshToken(string refreshToken);
        void RevokeRefreshToken();
    }
}
