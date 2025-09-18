using MusicSharing.Api.Models;

namespace MusicSharing.Api.DTOs
{
    public class CreateUserDto
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public UserRole Role { get; set; } = UserRole.User;
    }
}
