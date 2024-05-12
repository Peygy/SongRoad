using MainApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    [Route("api/music")]
    [ApiController]
    [Authorize(Roles = UserRoles.User)]
    public class ApiMusicController : ControllerBase
    {

    }
}
