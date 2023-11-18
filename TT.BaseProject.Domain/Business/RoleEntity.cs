using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Business
{
    [Table("role")]
    public class RoleEntity
    {
        [Key]
        public Guid role_id { get; set; }

        /// <summary>
        /// Tên vai trò
        /// </summary>
        public string role_name { get; set; }

        /// <summary>
        /// Là admin hay không
        /// </summary>
        public bool is_admin { get; set; }

        /// <summary>
        /// Quyền bị cấm
        /// Dạng SubsystemCode:[2,3]
        /// </summary>
        public Dictionary<string, IList<Permission>>? permissions { get; set; }
    }
}
