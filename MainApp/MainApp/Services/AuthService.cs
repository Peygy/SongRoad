using MainApp.Models;
using MainApp.Models.Service;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MainApp.Services
{
    internal class AuthService : IAuthService
    {
        private readonly UserManager<UserModel> userManager;
        private readonly IJwtGenService jwtService;
        private readonly ICookieService cookieService;
        private readonly IJwtDataService tokensDataService;

        public AuthService(UserManager<UserModel> userManager, IJwtGenService jwtService, IJwtDataService tokensDataService, ICookieService cookieService)
        {
            this.userManager = userManager;
            this.jwtService = jwtService;
            this.cookieService = cookieService;
            this.tokensDataService = tokensDataService;
        }


        public async Task<bool> UserRegister(RegisterModel newUser)
        {
            var user = new UserModel { UserName = newUser.UserName };

            var result = await userManager.CreateAsync(user, HashService.HashPassword(newUser.Password));
            if (result.Succeeded)
            {
                // Add role for user
                await userManager.AddToRoleAsync(user, UserRoles.User);
                // Generate access and refresh tokens
                var tokens = jwtService.GenerateJwtTokens(await GetUserClaimsAsync(user));
                // Add tokens to cookies and database
                await AddTokensToStorages(tokens, user);

                return true;
            }

            return false;
        }


        public async Task<bool> UserLogin(LoginModel loginUser)
        {
            var user = await userManager.FindByNameAsync(loginUser.UserName);

            if (user != null && HashService.VerifyHashedPassword(user.PasswordHash, loginUser.Password))
            {
                // Check user refresh tokens (max = 5)
                await tokensDataService.CheckUserRefreshTokensCountAsync(user);
                // Generate access and refresh tokens
                var tokens = jwtService.GenerateJwtTokens(await GetUserClaimsAsync(user));
                // Add tokens to cookies and database
                await AddTokensToStorages(tokens, user);

                return true;
            }

            return false;
        }

        public async Task<bool> ModeratorLogin(LoginModel loginModer)
        {
            return await LoginWithRole(loginModer, UserRoles.Moderator);
        }

        public async Task<bool> AdminLogin(LoginModel loginModer)
        {
            return await LoginWithRole(loginModer, UserRoles.Admin);
        }


        // Login with Moder/Admin roles
        private async Task<bool> LoginWithRole(LoginModel loginModel, string role)
        {
            var roles = await GetStaffRoles(loginModel.UserName);

            if (roles.Contains(role))
            {
                return await UserLogin(loginModel);
            }

            return false;
        }
        // Get moder/admin roles
        private async Task<IList<string>> GetStaffRoles(string username)
        {
            var user = await userManager.FindByNameAsync(username);

            if (user != null)
            {
                return await userManager.GetRolesAsync(user);
            }

            return new List<string>();
        }
        // Get user principals data
        private async Task<List<Claim>> GetUserClaimsAsync(UserModel user)
        {
            var authClaims = new List<Claim>
            {
                new (ClaimTypes.Name, user.UserName),
                new (ClaimTypes.NameIdentifier, user.Id)
            };

            // Adding roles for jwt token
            foreach (var role in await userManager.GetRolesAsync(user))
            {
                authClaims.Add(new(ClaimTypes.Role, role));
            }

            return authClaims;
        }
        // Add tokens to cookies and database
        private async Task AddTokensToStorages((string, string) tokens, UserModel user)
        {
            // Params: access token, refresh token
            cookieService.SetTokens(tokens.Item1, tokens.Item2);
            // Add refresh token to database
            await tokensDataService.AddRefreshTokenAsync(tokens.Item2, user);
        }
    }
}
