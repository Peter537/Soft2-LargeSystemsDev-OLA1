using System;

namespace MyDRTVPrototype.Models
{
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