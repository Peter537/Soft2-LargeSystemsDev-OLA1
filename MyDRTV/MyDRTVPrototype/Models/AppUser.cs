using System;

namespace MyDRTVPrototype.Models
{
    /// <summary>
    /// Represents an end user of the system.  The name AppUser is used to
    /// avoid conflict with the built‑in User class and the page named User.  In
    /// the JSON data file users are still stored under the "Users" array.
    /// </summary>
    public class AppUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool Anonymous { get; set; }
        public string FavoriteGenre { get; set; } = string.Empty;
    }
}