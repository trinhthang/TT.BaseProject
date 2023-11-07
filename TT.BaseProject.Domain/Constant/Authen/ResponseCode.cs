using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Constant.Authen
{
    public static class AuthenResponseCode
    {
        public const string WRONGUSERORPASSWORD = "WRONG_USERNAME_OR_PASSWORD";
        public const string EXIST_USER = "EXIST_USER";
        public const string NOTEXIST_USER = "NOT_EXIST_USER";
        public const string INVALIDUSERORPASSWORD = "INVALID_USERNAME_OR_PASSWORD";

    }
}
