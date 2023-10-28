using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Cache.Constants;

namespace TT.BaseProject.Cache
{
    public abstract class BaseCached : ICached
    {
        #region public method

        public T Get<T>(string key, bool removeAfterGet = false)
        {
            var cacheKey = this.GetCacheKey(key);
            var result = this.GetCache<T>(cacheKey);

            if (removeAfterGet && !object.Equals(default(T), result))
            {
                this.RemoveCache(cacheKey);
            }
            return result;
        }

        public void Remove(string key)
        {
            var cacheKey = this.GetCacheKey(key);
            this.RemoveCache(cacheKey);
        }

        public void Set<T>(string key, T data, TimeSpan? expired = null)
        {
            var cacheKey = this.GetCacheKey(key);
            var cacheExpired = this.GetExpired(CacheType.Global, expired);
            this.SetCache(cacheKey, data, cacheExpired);
        }

        #endregion

        protected virtual string GetCacheKey(string key)
        {
            return key;
        }

        protected TimeSpan GetExpired(CacheType type, TimeSpan? expired)
        {
            if (expired != null)
            {
                return (TimeSpan)expired;
            }

            switch (type)
            {
                case CacheType.Global:
                    return TimeSpan.FromSeconds(4 * 60 * 60);
                case CacheType.Database:
                    return TimeSpan.FromSeconds(4 * 60 * 60);
                case CacheType.User:
                    return TimeSpan.FromSeconds(4 * 60 * 60);
                case CacheType.UserDatabase:
                    return TimeSpan.FromSeconds(4 * 60 * 60);
                case CacheType.Session:
                    return TimeSpan.FromSeconds(4 * 60 * 60);
            }

            return TimeSpan.FromSeconds(60 * 60);
        }

        protected virtual void SetCache<T>(string key, T data, TimeSpan expired)
        {
            throw new NotImplementedException();
        }

        protected virtual T GetCache<T>(string key)
        {
            throw new NotImplementedException();
        }

        protected virtual void RemoveCache(string key)
        {
            throw new NotImplementedException();
        }
    }
}
