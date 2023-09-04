using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Attributes
{
    /// <summary>
    /// Attribute đánh dấu trường dữ liệu sẽ xử lý version
    /// </summary>
    public class EditVersionAttribute : Attribute
    {
        /// <summary>
        /// Tên trường dữ liệu gốc
        /// </summary>
        public string DataField { get; set; } = "edit_version";
    }
}
