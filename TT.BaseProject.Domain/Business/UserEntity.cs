using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Business
{
    [Table("user")]
    public class UserEntity
    {
        [Key]
        public Guid user_id { get; set; }

        public string user_name { get; set; }

        public string salt { get; set; }

        public string password { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của người dùng
        /// </summary>
        public UserStatus status { get; set; }

    }
}
