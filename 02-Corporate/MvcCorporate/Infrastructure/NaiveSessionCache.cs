using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace MvcCorporate.Infrastructure
{
    public class NaiveSessionCache : TokenCache
    {
        private static readonly object FileLock = new object();

        private readonly string _cacheId;
        private readonly ISession _session;

        public NaiveSessionCache(string userId, ISession session)
        {
            _cacheId = userId + "_TokenCache";
            _session = session;
            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            lock (FileLock)
            {
                Deserialize(_session.Get(_cacheId));
            }
        }

        public void Persist()
        {
            lock (FileLock)
            {
                // reflect changes in the persistent store
                _session.Set(_cacheId, Serialize());

                // once the write operation took place, restore the HasStateChanged bit to false
                HasStateChanged = false;
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();

            lock (FileLock)
            {
                _session.Remove(_cacheId);
            }
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
