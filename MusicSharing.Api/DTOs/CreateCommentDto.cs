namespace MusicSharing.Api.DTOs;

public class CreateCommentDto
{
    public int SongId { get; set; }
    public string CommentText { get; set; } = default!;
    public bool IsAnonymous { get; set; } = false;
    public int? UserId { get; set; } // Optional, set if user is logged in
}