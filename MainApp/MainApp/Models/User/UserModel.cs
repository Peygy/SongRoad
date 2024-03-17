using Microsoft.AspNetCore.Identity;

namespace MainApp.Models
{
    public class UserModel : IdentityUser
    {
        public RefreshTokenModel? Token { get; set; }
    }
}