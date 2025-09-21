namespace MusicSharing.Api.DTOs;

public class CreateCommentDto
{
    public int? SongId { get; set; }
    public int? BlogPostId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; } = false;
    public int? UserId { get; set; }
}