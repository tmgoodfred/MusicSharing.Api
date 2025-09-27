using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.Services;

namespace MusicSharing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: api/admin/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _adminService.GetDashboardAsync();
            return Ok(dashboard);
        }

        // DELETE: api/admin/user/{id}
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var ok = await _adminService.DeleteUserAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE: api/admin/song/{id}
        [HttpDelete("song/{id}")]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var ok = await _adminService.DeleteSongAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE: api/admin/comment/{id}
        [HttpDelete("comment/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var ok = await _adminService.DeleteCommentAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE: api/admin/blog/{id}
        [HttpDelete("blog/{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var ok = await _adminService.DeleteBlogPostAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST: api/admin/promote/{id}
        [HttpPost("promote/{id}")]
        public async Task<IActionResult> PromoteUserToAdmin(int id)
        {
            var ok = await _adminService.PromoteUserToAdminAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
