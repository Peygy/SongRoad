using MainApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MainApp.Services
{
    internal class AuthService : IAuthService
    {
        private readonly UserManager<UserModel> userManager;
        private readonly ILogger<AuthService> log;
        private readonly IJwtService jwtService;

        public AuthService(UserManager<UserModel> userManager, ILogger<AuthService> log, IJwtService jwtService)
        {
            this.userManager = userManager;
            this.log = log;
            this.jwtService = jwtService;
        }


        // Get claims for jwt token
        private async Task<List<Claim>> GetUserClaimsAsync(UserModel user)
        {
            var authClaims = new List<Claim>
            { 
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id) 
            };

            // Adding roles for jwt token
            foreach (var role in await userManager.GetRolesAsync(user))
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            return authClaims;
        }
        public async Task<bool> UserRegister(RegisterModel newUser)
        {
            try
            {
                var user = new UserModel { UserName = newUser.UserName };

                var result = await userManager.CreateAsync(user, HashService.HashPassword(newUser.Password));
                if (result.Succeeded)
                {
                    // Add role for user
                    await userManager.AddToRoleAsync(user, UserRoles.User);
                    // Generate access and refresh tokens
                    jwtService.GenerateTokens(user, await GetUserClaimsAsync(user));

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }


        public async Task<bool> UserLogin(LoginModel loginUser)
        {
            try
            {
                var user = await userManager.FindByNameAsync(loginUser.UserName);
                if (user != null && HashService.VerifyHashedPassword(user.PasswordHash, loginUser.Password))
                {
                    await jwtService.CheckUserRefreshTokens(user);
                    jwtService.GenerateTokens(user, await GetUserClaimsAsync(user));

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }

        public async Task<bool> ModeratorLogin(LoginModel moder)
        {
            return true;
        }




        /*public bool AvailabilityCheck(string userLogin)
        {
            try
            {
                if (!userData.Users.Any(u => u.Login == userLogin)) return true;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }



        public async Task AddUserAsync(CreateUserModel newUser)
        {
            try
            {
                string newPassword = HashService.HashPassword(newUser.Password);      
                await userData.Users.AddAsync(
                    new User { Login = newUser.Login, Password = newPassword });
                await userData.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }



        public async Task<bool> UserAuthenticationAsync(User user)
        {
            try
            {
                if (userData.Users.Any(u => u.Login == user.Login))
                {
                    var userDb = await userData.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
                    if (HashService.VerifyHashedPassword(userDb.Password, user.Password)) return true;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }


        [Authorize(Roles = "editor")]
        public async Task<bool> CrewAuthenticationAsync(Admin admin, string role)
        {
            try
            {
                if (userData.Crew.Any(c => c.Login == admin.Login && c.Role == role))
                {
                    var admDb = await userData.Crew.FirstOrDefaultAsync(c => c.Login == admin.Login);
                    if (HashService.VerifyHashedPassword(admDb.Password, admin.Password)) 
                    { return true; }                 
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }*/
    }
}
