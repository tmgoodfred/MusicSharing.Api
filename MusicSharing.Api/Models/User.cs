using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ProfilePicturePath { get; set; }

    public bool EmailConfirmed { get; set; } = false;
}