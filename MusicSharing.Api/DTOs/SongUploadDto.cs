using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MusicSharing.Api.DTOs
{
    public class SongUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!; // Audio file

        public IFormFile? Artwork { get; set; } // Optional cover art

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Artist { get; set; } = null!;
    }
}
