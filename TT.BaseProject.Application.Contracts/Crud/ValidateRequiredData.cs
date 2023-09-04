using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Contracts.Crud
{
    /// <summary>
    /// Thông tin lỗi nhập thiếu dữ liệu
    /// </summary>
    public class ValidateRequiredData
    {
        /// <summary>
        /// Vị trí bản ghi
        /// Nếu là master sẽ để -1
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Danh sách các trường nhập thiếu
        /// </summary>
        public List<string> Fields { get; set; }
    }
}
