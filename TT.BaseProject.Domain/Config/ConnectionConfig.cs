using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Config
{
    public class ConnectionConfig
    {
        /// <summary>
        /// DB chung quản lý Management
        /// </summary>
        public string Master { get; set; }

        /// <summary>
        /// DB dữ liệu người dùng/đăng nhập
        /// </summary>
        public string Auth { get; set; }

        /// <summary>
        /// DB nghiệp vụ
        /// </summary>
        public string Business { get; set; }

    }
}
