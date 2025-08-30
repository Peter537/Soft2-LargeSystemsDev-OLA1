namespace MyDRTVPrototype.Models
{
    /// <summary>
    /// Represents a numeric rating (1–5 stars) given by a user to a movie.
    /// </summary>
    public class Rating
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int UserId { get; set; }
        public int Value { get; set; }
    }
}