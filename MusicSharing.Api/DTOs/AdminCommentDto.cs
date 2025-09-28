namespace MusicSharing.Api.DTOs
{
    public class AdminCommentDto
    {
        public int Id { get; set; }
        public int? SongId { get; set; }
        public int? BlogPostId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
    }
}
