using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TT.BaseProject.Extension;

namespace TT.BaseProject.Cache.Redis
{
    public static class RedisCacheFactory
    {
        public static void InjectDistCacheService(IServiceCollection services, IConfiguration configuration)
        {
            ExtensionFactory.InjectConfig<RedisCacheConfig>(configuration, "RedisCache", services);
            services.AddSingleton<IDistCached, RedisCached>();
        }
    }
}