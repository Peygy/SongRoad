using MainApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    [Route("api/crew")]
    [ApiController]
    [Authorize(Roles = UserRoles.Moderator)]
    public class ApiCrewController : ControllerBase
    {
        private readonly UserManager<UserModel> userManager;

        public ApiCrewController(UserManager<UserModel> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet("users")]
        public IEnumerable<UserModel> GetUsers()
        {
            return userManager.Users.ToList();
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ApiCrewController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ApiCrewController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ApiCrewController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
