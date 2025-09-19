using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;
using MusicSharing.Api.Services.Interfaces;

namespace MusicSharing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserService userService, MusicService musicService, IConfiguration config) : ControllerBase
    {
        private readonly UserService _userService = userService;
        private readonly IConfiguration _config = config;
        private readonly MusicService _musicService = musicService;

        // GET: api/user
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            var dtos = users.Select(u => new UserProfileDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            }).ToList();
            return Ok(dtos);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            var dto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
            return Ok(dto);
        }

        // POST: api/user
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = dto.PasswordHash,
                Role = dto.Role
            };
            var created = await _userService.CreateUserAsync(user);
            var result = new UserProfileDto
            {
                Id = created.Id,
                Username = created.Username,
                Email = created.Email,
                Role = created.Role.ToString(),
                CreatedAt = created.CreatedAt
            };
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
        }

        // PUT: api/user/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserProfileDto dto)
        {
            var updated = await _userService.UpdateUserProfileAsync(id, dto.Username, dto.Email);
            if (updated == null) return NotFound();
            var result = new UserProfileDto
            {
                Id = updated.Id,
                Username = updated.Username,
                Email = updated.Email,
                Role = updated.Role.ToString(),
                CreatedAt = updated.CreatedAt
            };
            return Ok(result);
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userService.AuthenticateAsync(dto.UsernameOrEmail, dto.Password);
            if (user == null)
                return Unauthorized("Invalid username/email or password.");

            var token = _userService.GenerateJwtToken(user, _config);
            return Ok(new { token });
        }

        [HttpGet("{id}/analytics")]
        public async Task<IActionResult> GetUserSongAnalytics(int id)
        {
            var analytics = await _musicService.GetUserSongAnalyticsAsync(id);
            return Ok(analytics);
        }
    }
}