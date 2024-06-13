using MainApp.Interfaces.Crew;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers.Api
{
    /// <summary>
    /// Api controller for crew commands endpoints
    /// </summary>
    [Route("api/crew/moder")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class ApiCrewManageController : ControllerBase
    {
        private readonly ICrewManageService crewManageService;

        public ApiCrewManageController(ICrewManageService crewManageService)
        {
            this.crewManageService = crewManageService;
        }

        [HttpGet]
        public async Task<IEnumerable<UserModel>> GetAllModers()
        {
            return await crewManageService.GetAllModersAsync();
        }

        [HttpPost("{moderId}")]
        public async Task<bool> AddModer(string moderId)
        {
            return await crewManageService.AddNewModerAsync(moderId);
        }

        [HttpDelete("{moderId}")]
        public async Task<bool> RemoveModer(string moderId)
        {
            return await crewManageService.RemoveModerAsync(moderId);
        }
    }
}
