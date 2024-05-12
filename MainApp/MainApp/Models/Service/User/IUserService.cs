using System.Security.Claims;

namespace MainApp.Models.Service
{
    public interface IUserService
    {
        List<string> GetUserRoles();
        Task<string?> GetUserId();
    }
}
