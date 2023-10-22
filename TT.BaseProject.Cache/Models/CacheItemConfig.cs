using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Cache.Models
{
    /// <summary>
    /// Cấu hình từng item cache
    /// </summary>
    public class CacheItemConfig
    {
        /// <summary>
        /// Có bật không
        /// true/null: Có
        /// false: Không
        /// </summary>
        public bool? Enable { get; set; }

        /// <summary>
        /// Định dạng của key lưu trữ
        /// Quyết định scope truy xuất cache
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Thời gian hết hạn của dist cache
        /// Nếu có khai báo thì mới lưu
        /// </summary>
        public int? DistSeconds { get; set; }

        /// <summary>
        /// Thời gian hết hạn của mem cache
        /// Nếu có khai báo thì mới lưu
        /// </summary>
        public int? MemSeconds { get; set; }

        /// <summary>
        /// Tên nhóm, nếu không khai báo thì lấy mặc định item đầu tiên trong config Cache.DistGroup
        /// </summary>
        public string DistGroup { get; set; }

        /// <summary>
        /// Nếu bật cờ này thì khi xóa cache mem sẽ gửi notify cho các host khác để xóa cache mem tương ứng không
        /// </summary>
        public bool InvalidMemory { get; set; }
    }
}
