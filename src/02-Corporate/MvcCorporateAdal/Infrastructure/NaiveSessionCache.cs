using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace MvcCorporateAdal.Infrastructure
{
    public class NaiveSessionCache : TokenCache
    {
        private static readonly object _fileLock = new object();
        private readonly string _userObjectId = string.Empty;
        private readonly string _cacheId = string.Empty;
        private readonly IMemoryCache _cache = null;

        public NaiveSessionCache(string userId, IMemoryCache cache)
        {
            _userObjectId = userId;
            _cacheId = _userObjectId + "_TokenCache";
            _cache = cache;
            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            lock (_fileLock)
            {
                Deserialize(_cache.Get<byte[]>(_cacheId));
            }
        }

        public void Persist()
        {
            lock (_fileLock)
            {
                // reflect changes in the persistent store
                _cache.Set<byte[]>(_cacheId, Serialize());
                // once the write operation took place, restore the HasStateChanged bit to false
                HasStateChanged = false;
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            _cache.Remove(_cacheId);
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
            Persist();
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after ADAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (HasStateChanged)
            {
                Persist();
            }
        }
    }
}
