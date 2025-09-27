using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;

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
                CreatedAt = u.CreatedAt,
                ProfilePicturePath = u.ProfilePicturePath,
                EmailConfirmed = u.EmailConfirmed
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
                CreatedAt = user.CreatedAt,
                ProfilePicturePath = user.ProfilePicturePath,
                EmailConfirmed = user.EmailConfirmed
            };
            return Ok(dto);
        }

        // POST: api/user
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateUserFormDto form)
        {
            string? profilePicturePath = null;
            if (form.ProfilePicture != null && form.ProfilePicture.Length > 0)
            {
                var uploadFolder = "/mnt/user/music-files/profile-pictures";
                profilePicturePath = await _userService.SaveProfilePictureAsync(form.ProfilePicture, uploadFolder);
            }

            var user = new User
            {
                Username = form.Username,
                Email = form.Email,
                PasswordHash = form.PasswordHash,
                Role = form.Role,
                ProfilePicturePath = profilePicturePath
            };

            try
            {
                var created = await _userService.CreateUserAsync(user);
                var token = await _userService.CreateEmailVerificationTokenAsync(created.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                if (token != null)
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    await _userService.SendVerificationEmailAsync(created.Id, baseUrl, token);
                }
                var result = new UserProfileDto
                {
                    Id = created.Id,
                    Username = created.Username,
                    Email = created.Email,
                    Role = created.Role.ToString(),
                    CreatedAt = created.CreatedAt,
                    ProfilePicturePath = created.ProfilePicturePath,
                    EmailConfirmed = created.EmailConfirmed
                };
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                // Email already exists
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateUserFormDto form)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            user.Username = form.Username;
            user.Email = form.Email;

            if (form.ProfilePicture != null && form.ProfilePicture.Length > 0)
            {
                var uploadFolder = "/mnt/user/music-files/profile-pictures";
                var path = await _userService.SaveProfilePictureAsync(form.ProfilePicture, uploadFolder);
                user.ProfilePicturePath = path;
            }

            var updated = await _userService.UpdateUserAsync(id, user);
            if (updated == null) return NotFound();

            var result = new UserProfileDto
            {
                Id = updated.Id,
                Username = updated.Username,
                Email = updated.Email,
                Role = updated.Role.ToString(),
                CreatedAt = updated.CreatedAt,
                ProfilePicturePath = updated.ProfilePicturePath,
                EmailConfirmed = updated.EmailConfirmed
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

            if (!user.EmailConfirmed)
                return StatusCode(StatusCodes.Status403Forbidden, "Email not verified.");

            var token = _userService.GenerateJwtToken(user, _config);
            return Ok(new { token, userId = user.Id });
        }

        [HttpGet("{id}/analytics")]
        public async Task<IActionResult> GetUserSongAnalytics(int id)
        {
            var analytics = await _musicService.GetUserSongAnalyticsAsync(id);
            return Ok(analytics);
        }

        [HttpPut("{id}/password")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordDto dto)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            user.PasswordHash = dto.NewPassword; // Will be hashed in the service
            var updated = await _userService.UpdateUserAsync(id, user);
            if (updated == null) return NotFound();

            return NoContent();
        }

        // GET: api/music/{id}/artwork
        [HttpGet("{id}/artwork")]
        public IActionResult GetArtwork(int id)
        {
            var song = _musicService.GetSongByIdAsync(id).Result;
            if (song == null || string.IsNullOrEmpty(song.ArtworkPath))
                return NotFound("Artwork not found.");

            if (!System.IO.File.Exists(song.ArtworkPath))
                return NotFound("Artwork file missing on disk.");

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(song.ArtworkPath, out var mimeType))
                mimeType = "application/octet-stream";

            return PhysicalFile(song.ArtworkPath, mimeType);
        }

        [HttpGet("{id}/profile-picture")]
        public async Task<IActionResult> GetProfilePicture(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null || string.IsNullOrEmpty(user.ProfilePicturePath))
                return NotFound("Profile picture not found.");

            if (!System.IO.File.Exists(user.ProfilePicturePath))
                return NotFound("Profile picture file missing on disk.");

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(user.ProfilePicturePath, out var mimeType))
                mimeType = "application/octet-stream";

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return PhysicalFile(user.ProfilePicturePath, mimeType);
        }

        [HttpPost("request-email-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestEmailVerification([FromBody] RequestEmailVerificationDto dto)
        {
            // Find user silently
            var user = (await _userService.SearchUsersAsync(dto.UsernameOrEmail, 1))
                .FirstOrDefault(u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

            if (user != null && !user.EmailConfirmed)
            {
                var token = await _userService.CreateEmailVerificationTokenAsync(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                if (token != null)
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    await _userService.SendVerificationEmailAsync(user.Id, baseUrl, token);
                }
            }
            // Generic response (avoid enumeration)
            return Ok(new { message = "If the account requires verification, an email has been sent." });
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            var ok = await _userService.VerifyEmailAsync(dto.Token);
            if (!ok) return BadRequest("Invalid or expired token.");
            return NoContent();
        }

        // GET /verify-email?token=XYZ
        [HttpGet("/verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailViaGet([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Missing token.");

            var ok = await _userService.VerifyEmailAsync(token);
            // Simple inline HTML response (avoid exposing whether token was previously used beyond success/failure).
            if (!ok)
            {
                const string failHtml = """
            <!DOCTYPE html>
            <html><head><title>Email Verification</title></head>
            <body style="font-family:Arial;">
                <h2>Email Verification</h2>
                <p>Verification link is invalid or expired.</p>
            </body></html>
            """;
                return new ContentResult { Content = failHtml, ContentType = "text/html", StatusCode = StatusCodes.Status400BadRequest };
            }

            const string successHtml = """
        <!DOCTYPE html>
        <html><head><title>Email Verified</title></head>
        <body style="font-family:Arial;">
            <h2>Email Verified</h2>
            <p>Your email has been successfully verified. You may close this tab and log in.</p>
        </body></html>
        """;
            return new ContentResult { Content = successHtml, ContentType = "text/html" };
        }

        // POST api/user/forgot-password
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            var raw = await _userService.CreatePasswordResetTokenAsync(
                dto.UsernameOrEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString());

            if (raw != null)
            {
                // Get user (optional) to send email
                var user = (await _userService.SearchUsersAsync(dto.UsernameOrEmail, 1))
                    .FirstOrDefault(u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);
                if (user != null)
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    await _userService.SendPasswordResetEmailAsync(user.Id, baseUrl, raw);
                }
            }

            return Ok(new { message = "If the account exists and is verified, a reset link has been sent." });
        }

        // POST api/user/reset-password
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var ok = await _userService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!ok) return BadRequest("Invalid or expired token.");
            return NoContent();
        }

        [HttpGet("/reset-password")]
        [AllowAnonymous]
        public IActionResult ResetPasswordLanding([FromQuery] string token)
        {
            // Just lightweight HTML explaining user should use the frontend app or POST.
            const string html = """
            <!DOCTYPE html><html><body style="font-family:Arial;">
            <h3>Password Reset</h3>
            <p>If you are seeing this directly, open the main app and paste your token.</p>
            </body></html>
            """;
            return Content(html, "text/html");
        }
    }
}