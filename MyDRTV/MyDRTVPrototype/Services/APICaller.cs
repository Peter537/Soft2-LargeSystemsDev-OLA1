using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyDRTVPrototype.Models;

namespace MyDRTVPrototype.Services
{
    /// <summary>
    /// Acts as a facade for the pages to interact with the CoreMoviePlayer.  In
    /// a real application this class would use HttpClient to call a REST API.
    /// Here it simply delegates to the CoreMoviePlayer service registered in DI.
    /// </summary>
    public class APICaller
    {
        private readonly CoreMoviePlayer _core;
        private readonly FakeAuthService _auth;

        public APICaller(CoreMoviePlayer core, FakeAuthService auth)
        {
            _core = core;
            _auth = auth;
        }

        public Task<List<Movie>> GetAllMoviesAsync() => _core.GetAllMoviesAsync();

        public Task<List<Movie>> SearchMoviesAsync(string query) => _core.SearchMoviesByTitleAsync(query);

        public async Task<Movie?> GetMovieDetailsAsync(int id)
        {
            return await _core.GetMovieByIdAsync(id);
        }

        public async Task<List<Comment>> GetCommentsAsync(int movieId)
        {
            return await _core.GetCommentsForMovieAsync(movieId);
        }

        public async Task<List<Rating>> GetRatingsAsync(int movieId)
        {
            return await _core.GetRatingsForMovieAsync(movieId);
        }

        public async Task<double> GetAverageRatingAsync(int movieId)
        {
            return await _core.GetAverageRatingForMovieAsync(movieId);
        }

        public async Task AddRatingAsync(int movieId, int value)
        {
            var rating = new Rating
            {
                MovieId = movieId,
                UserId = _auth.CurrentUserId,
                Value = value
            };
            await _core.AddRatingAsync(rating);
        }

        public async Task AddCommentAsync(int movieId, string content)
        {
            var comment = new Comment
            {
                MovieId = movieId,
                UserId = _auth.CurrentUserId,
                Content = content
            };
            await _core.AddCommentAsync(comment);
        }

        public Task<AppUser?> GetCurrentUserAsync() => _auth.GetCurrentUserAsync();

        public async Task UpdateCurrentUserAsync(AppUser user)
        {
            await _core.UpdateUserAsync(user);
        }

        public async Task<List<WatchlistEntry>> GetWatchlistAsync(string? state = null)
        {
            return await _core.GetWatchlistForUserAsync(_auth.CurrentUserId, state);
        }

        public async Task AddOrUpdateWatchlistAsync(int movieId, string state)
        {
            var entry = new WatchlistEntry
            {
                UserId = _auth.CurrentUserId,
                MovieId = movieId,
                State = state
            };
            await _core.AddOrUpdateWatchlistEntryAsync(entry);
        }
    }
}