using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = default!;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;
    [Required]
    public string PasswordHash { get; set; } = default!;
    [Required]
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // New property
    public string? ProfilePicturePath { get; set; }
}