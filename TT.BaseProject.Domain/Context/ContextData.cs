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

        /// <summary>
        /// Nếu làm multi tenant thì mới cần dùng đến Connection trong Context
        /// </summary>
        //public string Connection { get; set; }
    }
}
