using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Enum
{
    /// <summary>
    /// Trạng thái tài khoản người dùng
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// Chờ kích hoạt
        /// </summary>
        Wait = 0,

        /// <summary>
        /// Đã kích hoạt
        /// </summary>
        Active = 1,

        /// <summary>
        /// Ngừng kích hoạt
        /// </summary>
        Inactive = 2,
    }
}
