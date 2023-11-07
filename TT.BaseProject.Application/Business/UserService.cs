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
    public class UserService : CrudBaseService<IUserRepo, Guid, UserEntity, UserDtoEdit>, IUserService
    {
        public UserService(IUserRepo repo, IServiceProvider serviceProvider) : base(repo, serviceProvider)
        {
        }
    }
}
