using MainApp.Models;
using Microsoft.AspNetCore.Identity;

namespace MainApp.Data
{
    // Init first identity data (roles + admin)
    public static class IdentityInitializer
    {
        public static async Task InitializeAsync(UserManager<UserModel> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            // get credentials from configuration
            var name = configuration["Admin:Name"];
            var password = configuration["Admin:Password"];

            // Adding users roles
            if (await roleManager.FindByNameAsync(UserRoles.User) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            }
            if (await roleManager.FindByNameAsync(UserRoles.Moderator) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Moderator));
            }
            if (await roleManager.FindByNameAsync(UserRoles.Admin) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }

            // Adding initial admin
            if (await userManager.FindByNameAsync(name) == null)
            {
                var admin = new UserModel { UserName = name };

                var result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    // Add all roles for admin
                    await userManager.AddToRolesAsync(admin, new List<string>() {
                        UserRoles.User,
                        UserRoles.Moderator,
                        UserRoles.Admin
                    });
                }
            }
        }
    }
}
