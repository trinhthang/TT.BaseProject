using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Cache;

namespace TT.BaseProject.HostBase.Caches
{
    public class MicrosoftMemCached : BaseCached, IMemCached
    {
        private readonly IMemoryCache _memoryCache;

        public MicrosoftMemCached(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        protected override T GetCache<T>(string key)
        {
            return _memoryCache.Get<T>(key);
        }

        protected override void SetCache<T>(string key, T data, TimeSpan expired)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();

            cacheEntryOptions.SetAbsoluteExpiration(expired);

            // save data in cache
            _memoryCache.Set(key, data, cacheEntryOptions);
        }

        protected override void RemoveCache(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}
