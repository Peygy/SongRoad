using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers.Api
{
    [Route("api/crew/music")]
    [ApiController]
    [Authorize(Roles = UserRoles.Moderator)]
    public class ApiMusicManageController : ControllerBase
    {

    }
}
