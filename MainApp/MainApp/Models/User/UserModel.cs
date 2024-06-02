using Microsoft.AspNetCore.Identity;

namespace MainApp.Models
{
    /// <summary>
    /// Model of user
    /// </summary>
    public class UserModel : IdentityUser
    {
        public RefreshTokenModel? Token { get; set; }
        public UserRights? Rights { get; set; }
    }
}