using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace TT.BaseProject.Cache.Redis
{
    public class RedisCached : BaseCached, IDistCached
    {
        private readonly IDistributedCache _distributedCache;
        private static readonly Type STRING_TYPE = typeof(string);

        public RedisCached(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        protected override T GetCache<T>(string key)
        {
            string jsonText = _distributedCache.GetString(key);

            if (string.IsNullOrEmpty(jsonText)) return default;

            if (typeof(T) == STRING_TYPE)
            {
                return (T)(object)jsonText;
            }

            return JsonConvert.DeserializeObject<T>(jsonText);
        }

        protected override void SetCache<T>(string key, T data, TimeSpan expired)
        {
            string jsonText;

            if (data is string)
            {
                jsonText = (string)(object)data;
            }
            else
            {
                jsonText = JsonConvert.SerializeObject(data);
            }

            _distributedCache.SetString(key, jsonText, GetDistributedCacheEntryOptions(expired));
        }

        protected override void RemoveCache(string key)
        {
            _distributedCache.Remove(key);
        }

        private DistributedCacheEntryOptions GetDistributedCacheEntryOptions(TimeSpan expired)
        {
            return new DistributedCacheEntryOptions().SetAbsoluteExpiration(expired);
        }

    }
}