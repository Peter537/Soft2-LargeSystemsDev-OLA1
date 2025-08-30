using System;

namespace MyDRTVPrototype.Models
{
    /// <summary>
    /// Represents a comment left by a user on a movie.
    /// </summary>
    public class Comment
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}