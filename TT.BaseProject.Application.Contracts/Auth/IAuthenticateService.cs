using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Application.Contracts.Base;
using TT.BaseProject.Domain.Authen;
using TT.BaseProject.Domain.Business;

namespace TT.BaseProject.Application.Contracts.Auth
{
    public interface IAuthenticateService : IBaseService<UserEntity>
    {
        Task<UserEntity> GetUserByUserIdAsync(Guid id);

        Task<AuthenticateResponse> Register(AuthenticateRequest registerModel);

        Task<AuthenticateResponse> Login(AuthenticateRequest loginModel);

        Task<AuthenticateResponse> LoginWithGoogle(SocialAuthenticateRequest loginModel);

        Task<AuthenticateResponse> LoginWithFacebook(SocialAuthenticateRequest loginModel);

    }
}
