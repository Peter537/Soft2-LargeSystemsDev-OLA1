using System.Threading.Tasks;
using MyDRTVPrototype.Models;

namespace MyDRTVPrototype.Services
{
    /// <summary>
    /// A very simple authentication service used only for this prototype.  It
    /// always returns the same hard‑coded user ID and allows pages to fetch
    /// the corresponding User from the CoreMoviePlayer.  There is no
    /// validation or security.
    /// </summary>
    public class FakeAuthService
    {
        private readonly CoreMoviePlayer _core;

        public FakeAuthService(CoreMoviePlayer core)
        {
            _core = core;
        }

        /// <summary>
        /// The ID of the currently "authenticated" user.  In a real system
        /// this value would come from a JWT or session state.
        /// </summary>
        public int CurrentUserId => 1;

        /// <summary>
        /// Returns the <see cref="AppUser"/> representing the current user.
        ///
        /// In this prototype the person domain model is called <see cref="AppUser"/>
        /// to avoid conflicts with the built‑in ASP.NET <c>User</c> type and the
        /// Razor page named <c>UserProfile</c>.  This method simply delegates to
        /// <see cref="CoreMoviePlayer.GetUserByIdAsync(int)"/> to fetch the user
        /// information from the in‑memory data store.
        /// </summary>
        public Task<AppUser?> GetCurrentUserAsync() => _core.GetUserByIdAsync(CurrentUserId);
    }
}