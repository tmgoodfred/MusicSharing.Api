using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.DTOs;

public class ForgotPasswordRequestDto
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;
}