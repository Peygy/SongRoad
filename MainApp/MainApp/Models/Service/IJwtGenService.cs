﻿using System.Security.Claims;

namespace MainApp.Models
{
    public interface IJwtGenService
    {
        (string, string) GenerateJwtTokens(List<Claim> authClaims);
        bool ValidAccessToken(string accessToken);
        ClaimsPrincipal GetTokenUserClaims(string accessToken);
    }
}