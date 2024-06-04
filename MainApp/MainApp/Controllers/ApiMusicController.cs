using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    /// <summary>
    /// Api controller for actions with music tracks
    /// </summary>
    [Route("api/music")]
    [ApiController]
    [Authorize(Roles = UserRoles.User)]
    public class ApiMusicController : ControllerBase
    {

    }
}
