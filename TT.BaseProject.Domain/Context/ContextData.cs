using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Context
{
    public class ContextData
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        #region Nếu làm multi tenant, mỗi tenant 1 database thì mới cần dùng đến DatabaseId, Connection trong Context
        public Guid? DatabaseId { get; set; }

        public string Connection { get; set; } 
        #endregion
    }
}
