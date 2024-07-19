using MainApp.Models.User;
using MainApp.Services.Crew;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers.Api
{
    /// <summary>
    /// Api controller for managing crew
    /// </summary>
    [Route("api/crew/moder")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class ApiCrewManageController : ControllerBase
    {
        /// <summary>
        /// Service for performing actions over crew
        /// </summary>
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
        public async Task<bool> AddModer(string userId)
        {
            return await crewManageService.AddNewModerAsync(userId);
        }

        [HttpDelete("{moderId}")]
        public async Task<bool> RemoveModer(string moderId)
        {
            return await crewManageService.RemoveModerAsync(moderId);
        }
    }
}
