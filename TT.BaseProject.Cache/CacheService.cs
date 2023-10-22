using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TT.BaseProject.Cache.Constants;
using TT.BaseProject.Cache.Models;

namespace TT.BaseProject.Cache
{
    public class CacheService : ICacheService
    {
        private readonly CacheConfig _cacheConfig;
        private readonly IMemCached _memCached;
        private readonly Dictionary<string, IDistCached> _distCaches;
        private readonly IInvalidMemoryCacheService _invalidMemoryCacheService = null;

        private const string USER_ID = "{uid}";
        private const string DATABASE_ID = "{dbid}";
        private const string SESSION_ID = "{sid}";
        private const string GUID = "{guid}";
        private const string CUSTOM = "{custom}";

        public CacheService(CacheConfig cacheConfig, IMemCached memCached, Dictionary<string, IDistCached> distCaches, IInvalidMemoryCacheService invalidMemoryCacheService)
        {
            _cacheConfig = cacheConfig;
            _memCached = memCached;
            _distCaches = distCaches;
            _invalidMemoryCacheService = invalidMemoryCacheService;
        }

        public T Get<T>(CacheParam param, bool removeAfterGet = false)
        {
            var data = GetData<T>(param, removeAfterGet);
            if (data == null)
            {
                return default(T);
            }

            return data.Value;
        }

        public virtual CacheData<T> GetData<T>(CacheParam param, bool removeAfterGet = false)
        {
            var itemConfig = this.GetItemConfig(param.Name);
            if (itemConfig.Enable == false)
            {
                return null;
            }

            var cacheKey = this.GetCacheKey(itemConfig, param);

            CacheData<T> cacheData = null;

            // Uu tien doc Mem truoc
            if (itemConfig.MemSeconds > 0)
            {
                cacheData = _memCached.Get<CacheData<T>>(cacheKey, removeAfterGet);
            }

            // Co cau hinh dist thi doc dist
            if (cacheData == null && itemConfig.DistSeconds > 0)
            {
                var distCache = this.GetDistCached(itemConfig);
                cacheData = distCache.Get<CacheData<T>>(cacheKey, removeAfterGet);

                if (cacheData != null && itemConfig.MemSeconds > 0 && !removeAfterGet)
                {
                    _memCached.Set(cacheKey, cacheData, this.GetExpired(itemConfig.MemSeconds.Value));
                }
            }

            return cacheData;
        }

        public int Set<T>(CacheParam param, T data)
        {
            var itemConfig = this.GetItemConfig(param.Name);
            if (itemConfig.Enable == false)
            {
                return 0;
            }

            var cacheKey = this.GetCacheKey(itemConfig, param);
            var cacheData = this.CreateCacheObject(data);
            int result = 0;

            if (itemConfig.DistSeconds > 0)
            {
                var distCache = this.GetDistCached(itemConfig);
                distCache.Set(cacheKey, cacheData, this.GetExpired(itemConfig.DistSeconds.Value));
                result = itemConfig.DistSeconds.Value;
            }
            else if (itemConfig.MemSeconds > 0)
            {
                _memCached.Set(cacheKey, cacheData, this.GetExpired(itemConfig.MemSeconds.Value));
                result = itemConfig.MemSeconds.Value;
            }

            return result;
        }

        public void Remove(CacheParam param)
        {
            var itemConfig = this.GetItemConfig(param.Name);
            var cacheKey = this.GetCacheKey(itemConfig, param);

            if (itemConfig.MemSeconds > 0)
            {
                _memCached.Remove(cacheKey);

                if (itemConfig.InvalidMemory && _invalidMemoryCacheService != null)
                {
                    _invalidMemoryCacheService.InvalidAsync(cacheKey);
                }
            }

            if (itemConfig.DistSeconds > 0)
            {
                var distCache = this.GetDistCached(itemConfig);
                distCache.Remove(cacheKey);
            }
        }

        public bool TryGet<T>(CacheParam param, out T result, bool removeAfterGet = false)
        {
            var data = GetData<T>(param, removeAfterGet);

            if (data == null)
            {
                result = default(T);
                return false;
            }

            result = data.Value;
            return true;
        }

        public virtual CacheData<T> CreateCacheObject<T>(T data)
        {
            var obj = new CacheData<T>
            {
                Value = data
            };

            return obj;
        }

        protected virtual TimeSpan GetExpired(int expiredSeconds)
        {
            return TimeSpan.FromSeconds(expiredSeconds);
        }

        protected virtual IDistCached GetDistCached(CacheItemConfig itemConfig)
        {
            var cacheKey = string.IsNullOrEmpty(itemConfig.DistGroup) ? _cacheConfig.DistGroups[0] : itemConfig.DistGroup;
            if (!_distCaches.ContainsKey(cacheKey))
            {
                throw new KeyNotFoundException($"Thiếu cấu hình distCache cacheKey {cacheKey}");
            }

            return _distCaches[cacheKey];
        }

        public virtual string GetCacheKey(CacheItemConfig config, CacheParam param)
        {
            var cacheKey = config.Key;
            var regex = new Regex(@"{(\w+)}");
            var rgMatch = regex.Matches(cacheKey);

            if (rgMatch.Count > 0)
            {
                var invalidFields = new List<string>();
                for (var i = 0; i < rgMatch.Count; i++)
                {
                    var item = rgMatch[i].Value;
                    switch (item)
                    {
                        case USER_ID:
                            if (param.UserId != null)
                            {
                                cacheKey = cacheKey.Replace(USER_ID, param.UserId.ToString());
                            }
                            else
                            {
                                invalidFields.Add(item);
                            }
                            break;
                        case DATABASE_ID:
                            if (param.DatabaseId != null)
                            {
                                cacheKey = cacheKey.Replace(DATABASE_ID, param.DatabaseId.ToString());
                            }
                            else
                            {
                                invalidFields.Add(item);
                            }
                            break;
                        case SESSION_ID:
                            if (param.SessionId != null)
                            {
                                cacheKey = cacheKey.Replace(SESSION_ID, param.SessionId.ToString());
                            }
                            else
                            {
                                invalidFields.Add(item);
                            }
                            break;
                        case GUID:
                            if (param.Guid != null)
                            {
                                cacheKey = cacheKey.Replace(GUID, param.Guid.ToString());
                            }
                            else
                            {
                                invalidFields.Add(item);
                            }
                            break;
                        case CUSTOM:
                            if (param.Custom != null)
                            {
                                cacheKey = cacheKey.Replace(CUSTOM, param.Custom.ToString());
                            }
                            else
                            {
                                invalidFields.Add(item);
                            }
                            break;
                    }
                }

                if (invalidFields.Count > 0)
                {
                    throw new Exception($"Key cache {param.Name} thiếu tham số  {string.Join(",", invalidFields)}");
                }
            }

            return cacheKey.ToLower();
        }

        protected virtual CacheItemConfig GetItemConfig(CacheItemName name)
        {
            var cacheKey = name.ToString();
            if (!_cacheConfig.Items.ContainsKey(cacheKey))
            {
                throw new KeyNotFoundException($"Không tìm thấy cấu hình cache {cacheKey}");
            }

            var result = _cacheConfig.Items[cacheKey];
            if (string.IsNullOrEmpty(result.Key))
            {
                throw new KeyNotFoundException($"Thiếu cấu hình cache {cacheKey} format");
            }

            return result;
        }
    }
}
