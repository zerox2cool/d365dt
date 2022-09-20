using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace ZD365DT.DeploymentTool.Utils
{
    public static class CrmCacheManager
    {
        private static ObjectCache cache = MemoryCache.Default;

        #region Methods
        /// <summary>
        /// Method to add items to cache for a specified period
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expirationDays">The expiration hours.</param>
        /// <returns></returns>
        public static object AddOrReplaceExisting(string key, object value, string expirationHours)
        {
            object Contents = cache[key];
            double cacheExpirationHours;

            if (!double.TryParse(expirationHours, out cacheExpirationHours))
                cacheExpirationHours = 1;

            if (Contents == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(cacheExpirationHours);
                cache.Set(key, value, policy);
            }
            else if (Object.ReferenceEquals(Contents, value) != true)
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(cacheExpirationHours);
                cache.Remove(key);
                cache.Set(key, value, policy);
            }

            return Contents;
        }

        /// <summary>
        /// Method to read the Cached value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetExisting(string key)
        {
            object content = cache[key];

            if (content != null)
            {
                return content;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Clean the cache
        /// </summary>
        public static void Clear()
        {
            KeyValuePair<string, object>[] allCache = cache.ToArray();

            foreach (var contents in allCache)
            {
                cache.Remove(contents.Key);
            }
        }

        /// <summary>
        /// Get the count of cached elements
        /// </summary>
        /// <returns></returns>
        public static long GetCount()
        {
            return cache.GetCount();
        }

        /// <summary>
        /// Remove the mentioed key
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            cache.Remove(key);
        }
        #endregion
    }
}
