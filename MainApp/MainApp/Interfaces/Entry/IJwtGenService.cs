using System.Security.Claims;

namespace MainApp.Interfaces.Entry
{
    public interface IJwtGenService
    {
        // Generate access and refresh tokens
        (string, string) GenerateJwtTokens(List<Claim> authClaims);
        // Valid access token
        bool ValidAccessToken(string accessToken);
        // Get user's claims from access token
        ClaimsPrincipal GetTokenUserClaims(string accessToken);
    }
}
