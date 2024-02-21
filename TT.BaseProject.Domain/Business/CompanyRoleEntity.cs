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
    /// Vai trò trong công ty
    /// </summary>
    [Table("company_role")]
    public class CompanyRoleEntity : IRecordCreate, IRecordModify
    {
        [Key]
        public Guid company_role_id { get; set; }

        public string company_role_name { get; set; }

        public Guid company_id { get; set; }

        public bool is_admin { get; set; } = false;

        /// <summary>
        /// Chuỗi JSON quy định các quyền Thêm/Sửa/Xóa/... từng Subsystem
        /// </summary>
        public string permissions { get; set; }

        public DateTime? created_date { get; set; }

        public string create_by { get; set; }

        public DateTime? modified_date { get; set; }

        public string modified_by { get; set; }
    }
}
