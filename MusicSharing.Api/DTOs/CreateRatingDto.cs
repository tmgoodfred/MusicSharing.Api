namespace MusicSharing.Api.DTOs;

public class CreateRatingDto
{
    public int SongId { get; set; }
    public int UserId { get; set; }
    public int RatingValue { get; set; } // 1-5
}