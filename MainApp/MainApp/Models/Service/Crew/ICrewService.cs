using System.Security.Claims;

namespace MainApp.Models.Service
{
    public interface ICrewService
    {
        List<string> GetCrewRoles();
    }
}
