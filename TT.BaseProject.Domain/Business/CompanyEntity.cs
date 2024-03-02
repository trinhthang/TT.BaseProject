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
    /// Công ty
    /// </summary>
    [Table("company")]
    public class CompanyEntity : IRecordCreate, IRecordModify
    {
        [Key]
        public Guid company_id { get; set; }

        public string company_name { get; set; }

        public string description { get; set; }

        public string address { get; set; }

        /// <summary>
        /// ID người dùng tạo Công ty
        /// </summary>
        public Guid user_id { get; set; }

        public DateTime? created_date { get; set; }

        public string create_by { get; set; }

        public DateTime? modified_date { get; set; }

        public string modified_by { get; set; }
    }
}
