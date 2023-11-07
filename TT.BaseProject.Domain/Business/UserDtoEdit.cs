using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Entity;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Business
{
    public class UserDtoEdit : UserEntity, IRecordState, IRecordCreate, IRecordModify
    {
        public ModelState state { get; set; }

        public long RecordVersion { get; set; }

        public DateTime? created_date { get; set; }

        public string create_by { get; set; }

        public DateTime? modified_date { get; set; }

        public string modified_by { get; set; }
    }
}
