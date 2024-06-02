using MainApp.DTO.User;
using MainApp.Interfaces.Entry;
using MainApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MainApp.Services
{
    /// <summary>
    /// Class of authentication/authorize service for user
    /// </summary>
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

        /// <summary>
        /// Method for user's account registration
        /// </summary>
        /// <param name="newUser">Model of registrated user</param>
        /// <returns>State of user registration</returns>
        public async Task<bool> UserRegister(RegisterModelDTO newUser)
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
                var tokens = jwtGenService.GenerateJwtTokens(await CreateUserClaimsAsync(user));

                // Add tokens to cookies
                cookieService.SetTokens(tokens.Item1, tokens.Item2);
                // Add refresh token to database
                await jwtDataService.AddRefreshTokenAsync(tokens.Item2, user);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Method for user's login in account
        /// </summary>
        /// <param name="loginUser">Model of login user</param>
        /// <returns>State of user login in account</returns>
        public async Task<bool> UserLogin(LoginModelDTO loginUser)
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
                var tokens = jwtGenService.GenerateJwtTokens(await CreateUserClaimsAsync(user));

                // Add tokens to cookies
                cookieService.SetTokens(tokens.Item1, tokens.Item2);
                // Add refresh token to database
                await jwtDataService.AddRefreshTokenAsync(tokens.Item2, user);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Method for moderator account login
        /// </summary>
        /// <param name="loginModer">Model of login moderator</param>
        /// <returns>State of moderator login in account</returns>
        public async Task<bool> ModeratorLogin(LoginModelDTO loginModer)
        {
            return await LoginWithRole(loginModer, UserRoles.Moderator);
        }

        /// <summary>
        /// Method for administrator account login
        /// </summary>
        /// <param name="loginModer">Model of login administrator</param>
        /// <returns>State of administrator login in account</returns>
        public async Task<bool> AdminLogin(LoginModelDTO loginModer)
        {
            return await LoginWithRole(loginModer, UserRoles.Admin);
        }


        // Login with Moder/Admin roles
        /// <summary>
        /// Method for login moderator or administrator
        /// </summary>
        /// <param name="loginModel">Model of login moderator/administrator</param>
        /// <param name="role">Role of user</param>
        /// <returns>State of staff login in account</returns>
        private async Task<bool> LoginWithRole(LoginModelDTO loginModel, string role)
        {
            var roles = await GetStaffRoles(loginModel.UserName);

            if (roles.Contains(role))
            {
                return await UserLogin(loginModel);
            }

            return false;
        }
        /// <summary>
        /// Method for get staff roles from storage
        /// </summary>
        /// <param name="username">Username of staff user</param>
        /// <returns>List of staff user roles</returns>
        private async Task<IList<string>> GetStaffRoles(string username)
        {
            var user = await userManager.FindByNameAsync(username);

            if (user != null)
            {
                return await userManager.GetRolesAsync(user);
            }

            return new List<string>();
        }
        /// <summary>
        /// Method for create user personal data claims
        /// </summary>
        /// <param name="user">Model of created user</param>
        /// <returns>List of user data (id, roles, name and etc)</returns>
        private async Task<List<Claim>> CreateUserClaimsAsync(UserModel user)
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


        /// <summary>
        /// Method for logout user from self account
        /// </summary>
        /// <returns>Task object</returns>
        public async Task Logout()
        {
            var claims = jwtGenService.GetTokenUserClaims(cookieService.GetAccessToken()).Claims;
            var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            cookieService.DeleteTokens();
            await jwtDataService.RemoveRefreshTokenDataAsync(userIdClaim.Value);
        }
    }
}
