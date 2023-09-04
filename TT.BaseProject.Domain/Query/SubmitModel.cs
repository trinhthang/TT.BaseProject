using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Query
{
    /// <summary>
    /// Đối tượng Submit dữ liệu vào db
    /// </summary>
    public class SubmitModel
    {
        /// <summary>
        /// Dữ liệu bảng nào
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Trạng thái bản ghi
        /// </summary>
        public ModelState State { get; set; }

        /// <summary>
        /// Dữ liệu
        /// </summary>
        public List<Dictionary<string, object>> Datas { get; set; }

        /// <summary>
        /// Tên trường khóa chính
        /// </summary>
        public List<string> KeyFields { get; set; }
    }
}
