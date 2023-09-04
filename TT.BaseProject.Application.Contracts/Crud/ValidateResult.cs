using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Contracts.Crud
{
    /// <summary>
    /// Lớp này để chứa kết quả trả về cho api
    /// </summary>
    public class ValidateResult
    {
        /// <summary>
        /// Mã nghiệp vụ
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Loại validate
        /// </summary>
        public ValidateResultType Type { get; set; }

        /// <summary>
        /// Nội dung trả về client
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Dữ liệu kèm theo
        /// </summary>
        public object Data { get; set; }
    }
}
