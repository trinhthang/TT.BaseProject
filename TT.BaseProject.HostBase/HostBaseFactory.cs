using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TT.BaseProject.Cache;
using TT.BaseProject.Cache.InvalidMemory;
using TT.BaseProject.Cache.Models;
using TT.BaseProject.Cache.Redis;
using TT.BaseProject.Extension;
using TT.BaseProject.HostBase.Caches;
using TT.BaseProject.Storage;
using TT.BaseProject.Storage.FileSystem;
using TT.BaseProject.Storage.MinIo;

namespace TT.BaseProject.HostBase
{
    public static class HostBaseFactory
    {
        public static void InjectStorageService(IServiceCollection services, IConfiguration configuraion)
        {
            if ("MinIO".Equals(configuraion.GetSection("Storage:Type").Value, StringComparison.OrdinalIgnoreCase))
            {

                services.AddSingleton<IStorageService, MinIoStorageService>();
            }
            else
            {
                services.AddSingleton<IStorageService, FileStorageService>();
            }
        }

        public static void InjectCacheService(IServiceCollection services, IConfiguration configuraion)
        {
            services.AddMemoryCache();
            var sp = services.BuildServiceProvider();
            var memoryCache = sp.GetService<IMemoryCache>();
            var memCache = new MicrosoftMemCached(memoryCache);

            //dist cache
            var distCacheds = GetRedisCached(configuraion);
            IInvalidMemoryCacheService invalidMemoryCacheService = null;

            //service
            var config = ExtensionFactory.InjectConfig<CacheConfig>(configuraion, "Cache", services);

            if (config.Mem != null
                && config.Mem.InvalidCache != null
                && !string.IsNullOrWhiteSpace(config.Mem.InvalidCache.Type))
            {
                switch (config.Mem.InvalidCache.Type.Trim().ToLower())
                {
                    case "redis":
                        if (config.Mem.InvalidCache.Redis == null)
                        {
                            throw new Exception("Thiếu cấu hình config.Memory.InvalidCache.Redis");
                        }

                        invalidMemoryCacheService = new RedisInvalidMemoryCacheService(memCache, config.Mem.InvalidCache.Redis);
                        break;
                }
            }

            if (invalidMemoryCacheService != null)
            {
                // Bắt đầu lắng nghe từ host khác thông báo để xóa mem cache đi
                invalidMemoryCacheService.StartSubcribe();
            }

            services.AddSingleton<ICacheService>(new CacheService(config, memCache, distCacheds, invalidMemoryCacheService));
        }

        private static Dictionary<string, IDistCached> GetRedisCached(IConfiguration configuration)
        {
            var result = new Dictionary<string, IDistCached>();
            var distConfig = ExtensionFactory.InjectConfig<Dictionary<string, RedisCacheConfig>>(configuration, "Cache:Redis");
            foreach (var item in distConfig)
            {
                var provider = new RedisCache(item.Value);
                var instance = new RedisCached(provider);
                result[item.Key] = instance;
            }

            return result;
        }
    }
}
