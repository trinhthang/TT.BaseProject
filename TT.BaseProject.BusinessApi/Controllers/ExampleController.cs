using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TT.BaseProject.Application.Contracts.Business;
using TT.BaseProject.Domain.Business;
using TT.BaseProject.HostBase.Controller;

namespace TT.BaseProject.BusinessApi.Controllers
{
    public class ExampleController : BaseBusinessApi<IExampleService, Guid, ExampleEntity, ExampleDtoEdit>
    {
        public ExampleController(IExampleService service, IServiceProvider serviceProvider) : base(service, serviceProvider)
        {

        }
    }
}
