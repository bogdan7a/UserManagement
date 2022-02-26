using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Services.User;
using UserManagement.Dtos.User;
using UserManagement.Models;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;

        public UsersController(ILogger<UsersController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLogin user)
        {
            _logger.LogInformation($"--- Trying to log user {user.Email} ---");

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var tokens = await _userService.Login(user);
            return Ok(tokens);
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegister user)
        {
            _logger.LogInformation("--- Trying to register a new user ---");

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _userService.Register(user);
            return Ok(user);
        }


        [HttpPost]
        [Route("RefreshToken")]
        [Authorize(Roles = "Employee, Manager")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenApiModel)
        {
            _logger.LogInformation("--- Token refresh ---");
            var newToken = await _userService.RefreshToken(tokenApiModel);
            return Ok(newToken);
        }


        [HttpPost]
        [Route("RevokeToken")]
        [Authorize(Roles = "Employee, Manager")]
        public async Task<IActionResult> Revoke()
        {
            _logger.LogInformation("--- Token revoke ---");
            var userEmail = User.Identity.Name;
            if (userEmail == null)
            {
                return BadRequest(new { message = "Invalid client request" });
            }

            await _userService.Revoke(userEmail);
            return NoContent();
        }


        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Employee, Manager")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("--- Getting all users ---");
            var users = await _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("GetUserById")]
        [Authorize(Roles = "Employee, Manager")]
        public async Task<IActionResult> GetUserById(int id)
        {
            _logger.LogInformation($"--- Getting user with id: {id} ---");
            var user = await _userService.GetById(id);
            return Ok(user);
        }


        [HttpPut("ChangeUserRole")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ChangeUserRole(int id, Helpers.UserRole role)
        {
            _logger.LogInformation($"--- Trying to change the user role ---");
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _userService.ChangeUserRole(id, role);
            return Ok();
        }


        [HttpPut("UpdateUser")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdate user)
        {
            _logger.LogInformation($"--- Trying to update user with id: {id} ---");

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _userService.Update(id, user);
            return Ok(user);
        }


        [HttpDelete("DeleteUser")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation($"--- Trying to delete user with id: {id} ---");
            await _userService.Delete(id);
            return Ok(new { message = $"Department with id:{id} has been deleted" });
        }
    }
}
