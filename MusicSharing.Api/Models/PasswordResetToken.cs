using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSharing.Api.Models;

public class PasswordResetToken
{
    [Key] public int Id { get; set; }

    [Required] public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required, MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? UsedAtUtc { get; set; }

    [Required]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(45)]
    public string? RequestIp { get; set; }

    [MaxLength(256)]
    public string? UserAgent { get; set; }
}