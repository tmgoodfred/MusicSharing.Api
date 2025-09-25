using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.DTOs;

public class VerifyEmailDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}