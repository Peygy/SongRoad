using MainApp.DTO.User;
using MainApp.Models.User;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MainApp.Services.Entry
{
    /// <summary>
    /// Defines the contract for an authentication service that handles user registration, login, and logout operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a <paramref name="newUser"/> in the system.
        /// </summary>
        /// <param name="newUser">The data transfer object containing the user's registration information.</param>
        /// <returns>
        /// The <see cref="Task"/> representing the asynchronous operation, 
        /// with a boolean result indicating success or failure of the registration.
        /// </returns>
        Task<bool> UserRegister(RegisterModelDTO newUser);

        /// <summary>
        /// Authenticates a user in the system.
        /// </summary>
        /// <param name="newUser">The data transfer object containing the user's login credentials.</param>
        /// <returns>
        /// The <see cref="Task"/> representing the asynchronous operation, 
        /// with a boolean result indicating whether the login was successful.
        /// </returns>
        Task<bool> UserLogin(LoginModelDTO newUser);

        /// <summary>
        /// Logs out the current user from the system.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> representing the asynchronous logout operation,
        /// with a boolean result indicating whether the logout was successful.
        /// </returns>
        Task<bool> Logout();
    }

    public class AuthService : IAuthService
    {
        /// <summary>
        /// Provides management and operation functionality for user accounts.
        /// </summary>
        private readonly UserManager<UserModel> userManager;
        /// <summary>
        /// Service for generating and validating JWT tokens.
        /// </summary>
        private readonly IJwtGenService jwtGenService;
        /// <summary>
        /// Service for handling refresh tokens, including storing and retrieving them.
        /// </summary>
        private readonly IRefershTokenService jwtDataService;
        /// <summary>
        /// Service for managing cookies, including setting and removing authentication cookies.
        /// </summary>
        private readonly ICookieService cookieService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="userManager">Provides functionality for managing user accounts.</param>
        /// <param name="jwtGenService">Service for generating and validating JWT tokens.</param>
        /// <param name="jwtDataService">Service for handling refresh tokens.</param>
        /// <param name="cookieService">Service for managing authentication cookies.</param>
        public AuthService(
            UserManager<UserModel> userManager, 
            IJwtGenService jwtGenService, 
            IRefershTokenService jwtDataService, 
            ICookieService cookieService)
        {
            this.userManager = userManager;
            this.jwtGenService = jwtGenService;
            this.jwtDataService = jwtDataService;
            this.cookieService = cookieService;
        }

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
        /// Creates a list of claims for the specified user, including their username, user ID, and roles.
        /// </summary>
        /// <param name="user">The user for whom to create the claims.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of claims for the user.
        /// </returns>
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

        public async Task<bool> Logout()
        {
            var accessToken = cookieService.GetAccessToken();
            if (accessToken == null)
            {
                return false;
            }

            var claims = jwtGenService.GetTokenUserClaims(accessToken);
            if (claims != null)
            {
                var userIdClaim = claims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    cookieService.DeleteTokens();
                    await jwtDataService.RemoveRefreshTokenDataAsync(userIdClaim.Value);

                    return true;
                }
            }

            return false;
        }
    }
}
