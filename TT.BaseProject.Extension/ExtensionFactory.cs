using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace TT.BaseProject.Extension
{
    public static class ExtensionFactory
    {
        public static TConfig InjectConfig<TConfig>(this IConfiguration configuration, string configSection, IServiceCollection services = null)
        {
            var config = configuration.GetSection(configSection).Get<TConfig>();

            var jsonSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            Console.WriteLine($"------- {configSection}: {JsonConvert.SerializeObject(config, jsonSettings)}");

            if (config == null)
            {
                throw new Exception($"Missing config {configSection}");
            }

            if (services != null)
            {
                services.AddSingleton(typeof(TConfig), config);
            }

            return config;
        }
    }
}