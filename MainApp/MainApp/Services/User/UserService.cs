﻿using MainApp.Interfaces.Entry;
using MainApp.Interfaces.User;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MainApp.Services
{
    /// <summary>
    /// Class for service of user
    /// </summary>
    [Authorize(Roles = UserRoles.User)]
    public class UserService : IUserService
    {
        private readonly ICookieService cookieService;
        private readonly IJwtGenService jwtGenService;
        private readonly UserManager<UserModel> userManager;

        public UserService(ICookieService cookieService, IJwtGenService jwtGenService, UserManager<UserModel> userManager)
        {
            this.cookieService = cookieService;
            this.jwtGenService = jwtGenService;
            this.userManager = userManager;
        }


        /// <summary>
        /// Method for get user role
        /// </summary>
        /// <returns>List of user roles</returns>
        public List<string> GetUserRoles()
        {
            var claims = GetUserClaims();
            return claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
        }

        /// <summary>
        /// Method for get user identificator
        /// </summary>
        /// <returns>User identificator</returns>
        public async Task<string?> GetUserId()
        {
            var claims = GetUserClaims();
            var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value;
            var user = await userManager.FindByNameAsync(name);
            return user.Id;
        }

        /// <summary>
        /// Method for get user data
        /// </summary>
        /// <returns>User data collection</returns>
        private IEnumerable<Claim> GetUserClaims()
        {
            var accessToken = cookieService.GetAccessToken();
            return jwtGenService.GetTokenUserClaims(accessToken).Claims;
        }
    }
}
