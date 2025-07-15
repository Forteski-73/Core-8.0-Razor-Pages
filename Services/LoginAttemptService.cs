using Microsoft.Extensions.Caching.Memory;

namespace OXF.Services
{
    public class LoginAttemptService : ILoginAttemptService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _blockTime = TimeSpan.FromMinutes(15);

        public LoginAttemptService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public int GetAttempts(string key)
        {
            return _cache.TryGetValue(key, out int attempts) ? attempts : 0;
        }

        public void IncrementAttempts(string key)
        {
            var attempts = GetAttempts(key) + 1;
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _blockTime
            };
            _cache.Set(key, attempts, cacheEntryOptions);
        }

        public void ResetAttempts(string key)
        {
            _cache.Remove(key);
        }
    }
}
