using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.DTOs;

public class RequestEmailVerificationDto
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;
}