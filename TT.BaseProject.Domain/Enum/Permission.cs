using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Enum
{
    public enum ModelState
    {
        None = 0,

        Insert = 1,

        Update = 2,

        Delete = 3,

        Empty = 4,

        Duplicate = 5,

        OverWrite = 6
    }
}
