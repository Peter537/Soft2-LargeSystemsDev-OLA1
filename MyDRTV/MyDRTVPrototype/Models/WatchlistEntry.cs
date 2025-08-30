namespace MyDRTVPrototype.Models
{
    public class WatchlistEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public string State { get; set; } = string.Empty;
    }
}