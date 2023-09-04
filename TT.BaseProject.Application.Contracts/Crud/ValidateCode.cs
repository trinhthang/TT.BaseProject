using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Contracts.Crud
{
    public enum ValidateCode
    {
        /// <summary>
        /// Bắt buộc
        /// </summary>
        Required,

        /// <summary>
        /// Lệch phiên bản dữ liệu
        /// </summary>
        EditVersion,

        /// <summary>
        /// Trùng mã
        /// </summary>
        Duplicate,

        /// <summary>
        /// Phát sinh
        /// </summary>
        Arise
    }
}
