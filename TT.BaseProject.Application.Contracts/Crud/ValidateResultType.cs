using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Contracts.Crud
{
    public enum ValidateResultType
    {
        /// <summary>
        /// Lỗi
        /// </summary>
        Error = 0,

        /// <summary>
        /// Cảnh báo
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Confirm Yes/No
        /// </summary>
        Question = 2,

        /// <summary>
        /// Lỗi phát sinh dữ liệu
        /// </summary>
        Generation = 3
    }
}
