using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Application.Contracts.Business;
using TT.BaseProject.Application.Base;
using TT.BaseProject.Domain.Business;
using TT.BaseProject.Domain.MySql.Business;

namespace TT.BaseProject.Application.Business
{
    public class ExampleService : CrudBaseService<IExampleRepo, Guid, ExampleEntity, ExampleDtoEdit>, IExampleService
    {
        public ExampleService(IExampleRepo repo, IServiceProvider serviceProvider) : base(repo, serviceProvider)
        {
        }
    }
}
