namespace MyDRTVPrototype.Models
{
    /// <summary>
    /// Represents a mapping between a user and a movie with a particular
    /// viewing state.  State can be one of "Watchlist" (to watch later),
    /// "InProgress" (currently watching) or "Watched" (already seen).
    /// </summary>
    public class WatchlistEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public string State { get; set; } = string.Empty;
    }
}