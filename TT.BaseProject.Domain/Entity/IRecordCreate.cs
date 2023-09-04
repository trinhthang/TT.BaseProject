using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Entity
{
    public interface IRecordCreate
    {
        public DateTime? created_date { get; set; }

        public string create_by { get; set; }
    }
}
