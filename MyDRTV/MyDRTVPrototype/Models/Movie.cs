using System;

namespace MyDRTVPrototype.Models
{
    /// <summary>
    /// Represents a single film or TV programme.
    /// </summary>
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Genre { get; set; } = string.Empty;
        public double AverageRating { get; set; }
    }
}