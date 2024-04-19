using MainApp.Models.Service;
using Microsoft.AspNetCore.Mvc;

// Контроллер для проверки токенов JWT
namespace MainApp.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public readonly IJwtCheckService jwtCheckService;

        public TokenController(IJwtCheckService jwtCheckService)
        {
            this.jwtCheckService = jwtCheckService;
        }

        [HttpGet("access/check")]
        public bool CheckAccessToken()
        {
            jwtCheckService.CheckAccessToken();
            return true;
        }

        [HttpGet("refresh/check")]
        public bool CheckRefreshToken()
        {
            jwtCheckService.CheckRefreshToken();
            return true;
        }
    }
}
