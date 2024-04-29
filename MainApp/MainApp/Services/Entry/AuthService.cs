using MainApp.Models;
using MainApp.Models.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MainApp.Services
{
    internal class AuthService : IAuthService
    {
        private readonly UserManager<UserModel> userManager;
        private readonly IJwtGenService jwtGenService;
        private readonly IJwtDataService jwtDataService;
        private readonly ICookieService cookieService;

        public AuthService(UserManager<UserModel> userManager, IJwtGenService jwtService, IJwtDataService jwtDataService, ICookieService cookieService)
        {
            this.userManager = userManager;
            this.jwtGenService = jwtService;
            this.jwtDataService = jwtDataService;
            this.cookieService = cookieService;
        }


        public async Task<bool> UserRegister(RegisterModel newUser)
        {
            if (await userManager.FindByNameAsync(newUser.UserName) != null)
            {
                return false;
            }

            var user = new UserModel { UserName = newUser.UserName };
            var result = await userManager.CreateAsync(user, newUser.Password);

            if (result.Succeeded)
            {
                // Add role for user
                await userManager.AddToRoleAsync(user, UserRoles.User);
                // Generate access and refresh tokens
                var tokens = jwtGenService.GenerateJwtTokens(await GetUserClaimsAsync(user));

                // Add tokens to cookies
                cookieService.SetTokens(tokens.Item1, tokens.Item2);
                // Add refresh token to database
                await jwtDataService.AddRefreshTokenAsync(tokens.Item2, user);

                return true;
            }

            return false;
        }


        public async Task<bool> UserLogin(LoginModel loginUser)
        {
            var user = await userManager.FindByNameAsync(loginUser.UserName);

            if (user != null && await userManager.CheckPasswordAsync(user, loginUser.Password))
            {
                // Check user if banned
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Count() == 0)
                {
                    return true;
                }

                // Check user refresh tokens (max = 5)
                await jwtDataService.CheckUserRefreshTokensCountAsync(user);
                // Generate access and refresh tokens
                var tokens = jwtGenService.GenerateJwtTokens(await GetUserClaimsAsync(user));

                // Add tokens to cookies
                cookieService.SetTokens(tokens.Item1, tokens.Item2);
                // Add refresh token to database
                await jwtDataService.AddRefreshTokenAsync(tokens.Item2, user);

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


        // Logout
        public async Task Logout()
        {
            var claims = jwtGenService.GetTokenUserClaims(cookieService.GetAccessToken()).Claims;
            var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            cookieService.DeleteTokens();
            await jwtDataService.RemoveRefreshTokenDataAsync(userIdClaim.Value);
        }
    }
}
