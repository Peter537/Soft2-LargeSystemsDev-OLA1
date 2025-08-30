using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MyDRTVPrototype.Models;

namespace MyDRTVPrototype.Services
{
    public class CoreMoviePlayer
    {
        private const string DataFilePath = "Data/data.json";

        private class DataStore
        {
            public List<Movie> Movies { get; set; } = new();
            public List<AppUser> Users { get; set; } = new();
            public List<Rating> Ratings { get; set; } = new();
            public List<Comment> Comments { get; set; } = new();
            public List<WatchlistEntry> WatchlistEntries { get; set; } = new();
        }

        private readonly DataStore _store;
        private readonly string _absolutePath;

        public CoreMoviePlayer()
        {
            // Compute the absolute path so that file IO works both in development and when published.
            _absolutePath = Path.Combine(AppContext.BaseDirectory, DataFilePath);

            if (!File.Exists(_absolutePath))
            {
                throw new FileNotFoundException($"Could not find data file at '{_absolutePath}'.");
            }

            var json = File.ReadAllText(_absolutePath);
            _store = JsonSerializer.Deserialize<DataStore>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new DataStore();
        }

        /// <summary>
        /// Returns all movies sorted alphabetically by title.
        /// </summary>
        public Task<List<Movie>> GetAllMoviesAsync()
        {
            var movies = _store.Movies.OrderBy(m => m.Title).ToList();
            return Task.FromResult(movies);
        }

        /// <summary>
        /// Returns a single movie by its identifier.
        /// </summary>
        public Task<Movie?> GetMovieByIdAsync(int id)
        {
            var movie = _store.Movies.FirstOrDefault(m => m.Id == id);
            return Task.FromResult(movie);
        }

        /// <summary>
        /// Returns movies whose titles contain the given query (case insensitive).
        /// </summary>
        public Task<List<Movie>> SearchMoviesByTitleAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return GetAllMoviesAsync();
            }

            query = query.Trim().ToLowerInvariant();
            var results = _store.Movies
                .Where(m => m.Title.ToLowerInvariant().Contains(query))
                .OrderBy(m => m.Title)
                .ToList();
            return Task.FromResult(results);
        }

        /// <summary>
        /// Returns all ratings for a given movie.
        /// </summary>
        public Task<List<Rating>> GetRatingsForMovieAsync(int movieId)
        {
            var list = _store.Ratings.Where(r => r.MovieId == movieId).ToList();
            return Task.FromResult(list);
        }

        /// <summary>
        /// Returns the average rating for a movie, or 0.0 if there are no ratings.
        /// </summary>
        public async Task<double> GetAverageRatingForMovieAsync(int movieId)
        {
            var ratings = await GetRatingsForMovieAsync(movieId);
            if (ratings.Count == 0)
            {
                return 0.0;
            }
            return ratings.Average(r => r.Value);
        }

        /// <summary>
        /// Adds a rating and updates the movie's AverageRating.  The ID of the
        /// rating is assigned automatically.
        /// </summary>
        public async Task AddRatingAsync(Rating rating)
        {
            rating.Id = _store.Ratings.Count > 0 ? _store.Ratings.Max(r => r.Id) + 1 : 1;
            _store.Ratings.Add(rating);

            // Update average rating on the movie itself
            var movie = _store.Movies.FirstOrDefault(m => m.Id == rating.MovieId);
            if (movie != null)
            {
                movie.AverageRating = await GetAverageRatingForMovieAsync(rating.MovieId);
            }
            await SaveChangesAsync();
        }

        /// <summary>
        /// Adds a new comment to a movie.  The ID of the comment is assigned
        /// automatically.
        /// </summary>
        public async Task AddCommentAsync(Comment comment)
        {
            comment.Id = _store.Comments.Count > 0 ? _store.Comments.Max(c => c.Id) + 1 : 1;
            comment.Timestamp = DateTime.UtcNow;
            _store.Comments.Add(comment);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Returns all comments for a movie ordered by timestamp descending.
        /// </summary>
        public Task<List<Comment>> GetCommentsForMovieAsync(int movieId)
        {
            var list = _store.Comments
                .Where(c => c.MovieId == movieId)
                .OrderByDescending(c => c.Timestamp)
                .ToList();
            return Task.FromResult(list);
        }

        /// <summary>
        /// Returns a user by ID.
        /// </summary>
        public Task<AppUser?> GetUserByIdAsync(int id)
        {
            var user = _store.Users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult<AppUser?>(user);
        }

        /// <summary>
        /// Saves changes made to a user back to the JSON file.
        /// </summary>
        public async Task UpdateUserAsync(AppUser user)
        {
            var existing = _store.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existing == null) return;
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.UserName = user.UserName;
            existing.BirthDate = user.BirthDate;
            existing.Address = user.Address;
            existing.Anonymous = user.Anonymous;
            existing.FavoriteGenre = user.FavoriteGenre;
            await SaveChangesAsync();
        }

        /// <summary>
        /// Returns all watchlist entries for a user.  You can filter by state (e.g.
        /// "Watchlist", "InProgress", "Watched") by passing a state string;
        /// otherwise all entries are returned.
        /// </summary>
        public Task<List<WatchlistEntry>> GetWatchlistForUserAsync(int userId, string? state = null)
        {
            var list = _store.WatchlistEntries.Where(w => w.UserId == userId);
            if (!string.IsNullOrWhiteSpace(state))
            {
                list = list.Where(w => w.State.Equals(state, StringComparison.OrdinalIgnoreCase));
            }
            return Task.FromResult(list.ToList());
        }

        /// <summary>
        /// Adds a movie to a user's watchlist.  If the entry already exists with a
        /// different state it is updated instead.
        /// </summary>
        public async Task AddOrUpdateWatchlistEntryAsync(WatchlistEntry entry)
        {
            // Find an existing entry with the same user, movie and state
            var existingSameState = _store.WatchlistEntries
                .FirstOrDefault(w => w.UserId == entry.UserId && w.MovieId == entry.MovieId &&
                                     w.State.Equals(entry.State, StringComparison.OrdinalIgnoreCase));
            if (existingSameState != null)
            {
                // No need to add duplicate entry
                return;
            }

            // Otherwise create a new entry even if a different state exists.  This allows
            // a movie to appear in multiple lists (e.g. InProgress and Watched).
            entry.Id = _store.WatchlistEntries.Count > 0 ? _store.WatchlistEntries.Max(w => w.Id) + 1 : 1;
            _store.WatchlistEntries.Add(entry);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Persists the current data store back to the JSON file.
        /// </summary>
        private Task SaveChangesAsync()
        {
            var json = JsonSerializer.Serialize(_store, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_absolutePath, json);
            return Task.CompletedTask;
        }
    }
}