using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Cache.Constants;

namespace TT.BaseProject.Cache.Models
{
    public class CacheParam
    {
        /// <summary>
        /// Tên cache
        /// </summary>
        public CacheItemName Name { get; set; }

        /// <summary>
        /// Người dùng
        /// </summary>
        public object UserId { get; set; }

        /// <summary>
        /// Dữ liệu
        /// </summary>
        public object DatabaseId { get; set; }

        /// <summary>
        /// Phiên đăng nhập
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Thông tin Guid, nếu không truyền thì khi sử dụng sẽ new
        /// </summary>
        public Guid? Guid { get; set; }

        /// <summary>
        /// Giá trị custom này cho tùy chỉnh
        /// </summary>
        public string Custom { get; set; }
    }
}
