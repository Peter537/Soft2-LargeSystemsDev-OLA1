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

        public Task<List<Movie>> GetAllMoviesAsync()
        {
            var movies = _store.Movies.OrderBy(m => m.Title).ToList();
            return Task.FromResult(movies);
        }

        public Task<Movie?> GetMovieByIdAsync(int id)
        {
            var movie = _store.Movies.FirstOrDefault(m => m.Id == id);
            return Task.FromResult(movie);
        }

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

        public Task<List<Rating>> GetRatingsForMovieAsync(int movieId)
        {
            var list = _store.Ratings.Where(r => r.MovieId == movieId).ToList();
            return Task.FromResult(list);
        }

        public async Task<double> GetAverageRatingForMovieAsync(int movieId)
        {
            var ratings = await GetRatingsForMovieAsync(movieId);
            if (ratings.Count == 0)
            {
                return 0.0;
            }
            return ratings.Average(r => r.Value);
        }

        public async Task AddRatingAsync(Rating rating)
        {
            rating.Id = _store.Ratings.Count > 0 ? _store.Ratings.Max(r => r.Id) + 1 : 1;
            _store.Ratings.Add(rating);

            var movie = _store.Movies.FirstOrDefault(m => m.Id == rating.MovieId);
            if (movie != null)
            {
                movie.AverageRating = await GetAverageRatingForMovieAsync(rating.MovieId);
            }
            await SaveChangesAsync();
        }

        public async Task AddCommentAsync(Comment comment)
        {
            comment.Id = _store.Comments.Count > 0 ? _store.Comments.Max(c => c.Id) + 1 : 1;
            comment.Timestamp = DateTime.UtcNow;
            _store.Comments.Add(comment);
            await SaveChangesAsync();
        }

        public Task<List<Comment>> GetCommentsForMovieAsync(int movieId)
        {
            var list = _store.Comments
                .Where(c => c.MovieId == movieId)
                .OrderByDescending(c => c.Timestamp)
                .ToList();
            return Task.FromResult(list);
        }

        public Task<AppUser?> GetUserByIdAsync(int id)
        {
            var user = _store.Users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult<AppUser?>(user);
        }

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
        public Task<List<WatchlistEntry>> GetWatchlistForUserAsync(int userId, string? state = null)
        {
            var list = _store.WatchlistEntries.Where(w => w.UserId == userId);
            if (!string.IsNullOrWhiteSpace(state))
            {
                list = list.Where(w => w.State.Equals(state, StringComparison.OrdinalIgnoreCase));
            }
            return Task.FromResult(list.ToList());
        }

        public async Task AddOrUpdateWatchlistEntryAsync(WatchlistEntry entry)
        {
            var existingSameState = _store.WatchlistEntries
                .FirstOrDefault(w => w.UserId == entry.UserId && w.MovieId == entry.MovieId &&
                                     w.State.Equals(entry.State, StringComparison.OrdinalIgnoreCase));
            if (existingSameState != null)
            {
                return;
            }

            entry.Id = _store.WatchlistEntries.Count > 0 ? _store.WatchlistEntries.Max(w => w.Id) + 1 : 1;
            _store.WatchlistEntries.Add(entry);
            await SaveChangesAsync();
        }

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