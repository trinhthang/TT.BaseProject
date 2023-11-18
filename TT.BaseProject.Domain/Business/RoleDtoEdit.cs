using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Entity;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Business
{
    public class RoleDtoEdit : RoleEntity, IRecordState
    {
        public ModelState state { get; set; }
    }
}
