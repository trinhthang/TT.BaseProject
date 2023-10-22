using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Cache.Constants
{
    /// <summary>
    /// Tên cache khai báo trong file Cache.json
    /// </summary>
    public enum CacheItemName
    {
        /// <summary>
        /// Thông tin phiên đăng nhập
        /// </summary>
        LoginSession,

        /// <summary>
        /// Chuỗi kết nối vào database
        /// </summary>
        DatabaseConnection,

        /// <summary>
        /// Câu lệnh sql insert, update
        /// </summary>
        SqlInsert,
        SqlUpdate,
        SqlDelete,
    }
}
