using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Config;

namespace TT.BaseProject.Domain.MySql
{
    public abstract class MySqlAuthRepo : MySqlRepo
    {
        public MySqlAuthRepo(IOptions<ConnectionConfig> config, IServiceProvider serviceProvider) : base(config.Value.Auth, serviceProvider)
        {

        }
    }
}
