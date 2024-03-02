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
    /// Người dùng tương ứng vai trò nào
    /// </summary>
    [Table("user_company_role")]
    public class UserCompanyRoleEntity : IRecordCreate, IRecordModify
    {
        [Key]
        public Guid user_company_role_id { get; set; }

        public Guid company_role_id { get; set; }

        public Guid user_id { get; set; }

        public DateTime? created_date { get; set; }

        public string create_by { get; set; }

        public DateTime? modified_date { get; set; }

        public string modified_by { get; set; }
    }
}
