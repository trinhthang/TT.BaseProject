using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Business
{
    [Table("user_role")]
    public class UserRoleEntity
    {
        [Key]
        public Guid user_role_id { get; set; }

        public Guid user_id { get; set; }

        public Guid role_id { get; set; }
    }
}
