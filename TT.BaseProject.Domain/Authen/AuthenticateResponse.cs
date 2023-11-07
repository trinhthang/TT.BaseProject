using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Business;

namespace TT.BaseProject.Domain.Authen
{
    public class AuthenticateResponse
    {
        /// <summary>
        /// Thành công/Thất bại
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Giá trị nằm trong AuthenResponseCode constants
        /// </summary>
        public string ResponseCode { get; set; }


        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Token { get; set; }

        public AuthenticateResponse(bool success, string responseCode)
        {
            Success = success;
            ResponseCode = responseCode;
        }

        public AuthenticateResponse(bool success, string responseCode, UserEntity user, string token)
        {
            Success = success;
            ResponseCode = responseCode;
            Id = user.user_id;
            Username = user.user_name;
            Token = token;
        }
    }
}
