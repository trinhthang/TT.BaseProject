using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Authen
{
    /// <summary>
    /// Model đăng nhập bằng tài khoản chương trình
    /// </summary>
    public class AuthenticateRequest
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }

    /// <summary>
    /// Model đăng nhập bằng tài khoản mạng xã hội
    /// </summary>
    public class SocialAuthenticateRequest
    {
        public string Token { get; set; }
    }
}
