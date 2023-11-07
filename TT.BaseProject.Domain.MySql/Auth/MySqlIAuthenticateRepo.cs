using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TT.BaseProject.Domain.Authen;
using TT.BaseProject.Domain.Business;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Context;

namespace TT.BaseProject.Domain.MySql.Business
{
    public class MySqlIAuthenticateRepo : MySqlAuthRepo, IAuthenticateRepo
    {
        protected readonly IOptions<ConnectionConfig> _config;

        public MySqlIAuthenticateRepo(IOptions<ConnectionConfig> config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            _config = config;
        }
    }
}
