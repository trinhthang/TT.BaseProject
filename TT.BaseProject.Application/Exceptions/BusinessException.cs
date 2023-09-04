using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Exceptions
{
    public class BusinessException : Exception
    {
        /// <summary>
        /// Nếu có thông tin này trả về thì sẽ hiển thị cho người dùng
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Mã lỗi
        /// Phần lớn sẽ sử dụng để Client dựa vào để hiển thị message tương ứng
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Dữ liệu trả về kèm để client hiển thị thông báo hoặc callback tương ứng
        /// </summary>
        public object ErrorData { get; set; }

        public string GetClientReturn()
        {
            return JsonConvert.SerializeObject(new
            {
                Error = this.ErrorCode,
                Data = this.ErrorData,
                Message = this.ErrorMessage
            });
        }
    }
}
