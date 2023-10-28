using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Cache.Constants
{
    public enum CacheType
    {
        /// <summary>
        /// Dùng chung toàn ứng dụng
        /// </summary>
        Global,

        /// <summary>
        /// Theo dữ liệu
        /// </summary>
        Database,

        /// <summary>
        /// Theo người dùng
        /// </summary>
        User,

        /// <summary>
        /// Theo người dùng trong dữ liệu
        /// </summary>
        UserDatabase,

        /// <summary>
        /// Theo phiên đăng nhập
        /// </summary>
        Session
    }
}
