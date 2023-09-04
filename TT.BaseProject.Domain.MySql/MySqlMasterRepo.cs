using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Config;

namespace TT.BaseProject.Domain.MySql
{
    public abstract class MySqlMasterRepo : MySqlRepo
    {
        public MySqlMasterRepo(IOptions<ConnectionConfig> config, IServiceProvider serviceProvider) : base(config.Value.Master, serviceProvider)
        {

        }
    }
}
