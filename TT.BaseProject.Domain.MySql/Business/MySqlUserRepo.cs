using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TT.BaseProject.Domain.Business;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Context;

namespace TT.BaseProject.Domain.MySql.Business
{
    public class MySqlUserRepo : MySqlBusinessRepo, IUserRepo
    {
        protected readonly IContextService _contextService;
        protected readonly IOptions<ConnectionConfig> _config;

        public MySqlUserRepo(IOptions<ConnectionConfig> config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            _config = config;
            _contextService = serviceProvider.GetRequiredService<IContextService>();
        }
    }
}
