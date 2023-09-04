using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Attributes
{
    /// <summary>
    /// Thông tin tham chiếu với dữ liệu gốc
    /// </summary>
    public interface IMasterRefAttribute
    {
        /// <summary>
        /// Tên trường mapping với master key
        /// </summary>
        public string MasterKeyField { get; set; }

        /// <summary>
        /// Kiểu dữ liệu
        /// </summary>
        public Type Type { get; set; }
    }
}
