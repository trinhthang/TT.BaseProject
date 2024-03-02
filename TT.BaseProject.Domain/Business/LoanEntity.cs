using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Entity;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Business
{
    // <summary>
    /// Khoản vay
    /// </summary>
    [Table("bus_loan")]
    public class LoanEntity : IRecordCreate, IRecordModify
    {
        [Key]
        public Guid loan_id { get; set; }

        public string loan_name { get; set; }

        // <summary>
        /// Loại vay: Vay lãi theo ngày | Lãi xuất % | ...
        /// </summary>

        public int loan_type { get; set; } = 0;

        // <summary>
        /// Trạng thái khoản vay: Mới | Trả xong | Trả nhưng trễ hạn | Nợ trễ hạn ...
        /// </summary>
        public int loan_status { get; set; } = 0;

        // <summary>
        /// Số tiền vay
        /// </summary>

        public decimal? loan_amount { get; set; }

        // <summary>
        /// Số tiền còn lại
        /// </summary>
        public decimal? loan_debt_amount { get; set; }

        // <summary>
        /// Ngày vay
        /// </summary>
        public DateTime? loan_date { get; set; }

        // <summary>
        /// Ngày hết hạn khoản vay
        /// </summary>
        public DateTime? loan_expire_date { get; set; }
        
        /// <summary>
        /// ID người vay
        /// </summary>
        public Guid customer_id { get; set; }

        /// <summary>
        /// ID người dùng tạo
        /// </summary>
        public Guid user_id { get; set; }

        /// <summary>
        /// ID công ty
        /// </summary>
        public Guid company_id { get; set; }

        public string description { get; set; }

        public DateTime? created_date { get; set; }

        public string create_by { get; set; }

        public DateTime? modified_date { get; set; }

        public string modified_by { get; set; }
    }
}
