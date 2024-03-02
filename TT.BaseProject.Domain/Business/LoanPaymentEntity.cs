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
    /// Đợt trả lãi/gốc vay
    /// </summary>
    [Table("bus_loan_payment")]
    public class LoanPaymentEntity : IRecordCreate, IRecordModify
    {
        [Key]
        public Guid loan_payment_id { get; set; }

        public string loan_payment_name { get; set; }

        public Guid loan_id { get; set; }

        // <summary>
        /// Số tiền trả đợt này
        /// </summary>
        public decimal loan_payment_amount { get; set; }

        // <summary>
        /// Ngày trả
        /// </summary>
        public DateTime? loan_payment_date { get; set; }

        // <summary>
        /// Hạn phải trả
        /// </summary>
        public DateTime? loan_payment_expire_date { get; set; }

        // <summary>
        /// Trạng thái: Mới | Hoàn thành | Trễ hạn ...
        /// </summary>
        public int loan_payment_status { get; set; } = 0;

        // <summary>
        /// Đợt 1, 2, 3 ...
        /// </summary>
        public int loan_payment_step { get; set; } = 1;

        // <summary>
        /// Mô tả thêm
        /// </summary>
        public string description { get; set; }

        public DateTime? created_date { get; set; }

        public string create_by { get; set; }

        public DateTime? modified_date { get; set; }

        public string modified_by { get; set; }
    }
}
