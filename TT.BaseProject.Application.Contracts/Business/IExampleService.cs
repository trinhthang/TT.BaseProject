using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Application.Contracts.Base;
using TT.BaseProject.Domain.Business;

namespace TT.BaseProject.Application.Contracts.Business
{
    public interface IExampleService : ICrudBaseService<Guid, ExampleEntity, ExampleDtoEdit>
    {
    }
}
