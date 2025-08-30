using System.Threading.Tasks;
using MyDRTVPrototype.Models;

namespace MyDRTVPrototype.Services
{
    public class FakeAuthService
    {
        private readonly CoreMoviePlayer _core;

        public FakeAuthService(CoreMoviePlayer core)
        {
            _core = core;
        }

        public int CurrentUserId => 1;

        public Task<AppUser?> GetCurrentUserAsync() => _core.GetUserByIdAsync(CurrentUserId);
    }
}