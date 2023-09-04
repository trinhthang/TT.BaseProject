using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Entity
{
    public interface IRecordState
    {
        ModelState state { get; set; }
    }
}
