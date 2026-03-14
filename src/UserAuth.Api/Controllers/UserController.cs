using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAuth.Api.DTOs.Users;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUser)
        {
            var user = new User
            {
                Email = createUser.Email,
                Password = createUser.Password
            };

            if (user == null)
                return BadRequest("User data is required.");

            var resposne = await _userService.CreateAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = resposne.Id }, user);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            var userDto = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                CreateAt = user.CreateAt,
                IsActive = user.IsActive
            };

            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();

            var userDto = users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                CreateAt = u.CreateAt,
                IsActive = u.IsActive
            });

            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUser)
        {
            var user = new User
            {
                Id = id,
                Password = updateUser.Password,
                IsActive = updateUser.IsActive,
                Role = updateUser.Role
            };

            if (user == null || user.Id != id)
                return BadRequest("Invalid user data.");

            var updated = await _userService.UpdateAsync(user);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }


    }
}
