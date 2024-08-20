using MainApp.DTO.User;
using MainApp.Models.User;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MainApp.Services.Entry
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        Task<bool> UserRegister(RegisterModelDTO newUser);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        Task<bool> UserLogin(LoginModelDTO newUser);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task Logout();
    }

    /// <summary>
    /// Class of authentication/authorize service for user
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<UserModel> userManager;
        private readonly IJwtGenService jwtGenService;
        private readonly IRefershTokenService jwtDataService;
        private readonly ICookieService cookieService;

        public AuthService(UserManager<UserModel> userManager, IJwtGenService jwtService, IRefershTokenService jwtDataService, ICookieService cookieService)
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
                if ((await userManager.GetRolesAsync(user)).Count() == 0)
                {
                    return false;
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
