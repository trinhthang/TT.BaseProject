using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TT.BaseProject.Domain.Config;
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
            //TODO
        }
    }
}
