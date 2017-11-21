using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IO;
using System.Security.Cryptography;

namespace WpfCorporateAdal
{
    // This is a simple persistent cache implementation for a desktop application.
    // It uses DPAPI for storing tokens in a local file.
    internal class FileCache : TokenCache
    {
        private static readonly object FileLock = new object();
        private readonly string _cacheFilePath;

        // Initializes the cache against a local file.
        // If the file is already rpesent, it loads its content in the ADAL cache
        public FileCache(string filePath = @".\TokenCache.dat")
        {
            _cacheFilePath = filePath;
            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;

            lock (FileLock)
            {
                Deserialize(File.Exists(_cacheFilePath)
                    ? ProtectedData.Unprotect(File.ReadAllBytes(_cacheFilePath), null, DataProtectionScope.CurrentUser)
                    : null);
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();

            lock (FileLock)
            {
                File.Delete(_cacheFilePath);
            }
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                Deserialize(File.Exists(_cacheFilePath)
                    ? ProtectedData.Unprotect(File.ReadAllBytes(_cacheFilePath), null, DataProtectionScope.CurrentUser)
                    : null);
            }
        }

        // Triggered right after ADAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changes in the persistent store
                    File.WriteAllBytes(_cacheFilePath, ProtectedData.Protect(Serialize(), null, DataProtectionScope.CurrentUser));
                    // once the write operation took place, restore the HasStateChanged bit to false
                    HasStateChanged = false;
                }
            }
        }
    }
}
